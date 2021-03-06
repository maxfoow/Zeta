﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Orion.Zeta.Core;
using Orion.Zeta.Methods.Dev.Shared;
using Orion.Zeta.Methods.Ui.Dev;
using Orion.Zeta.Methods.Ui.Dev.Tools.MVVM;
using Orion.Zeta.Persistence.LocalStorage;
using Orion.Zeta.Services;
using Orion.Zeta.Settings;
using Orion.Zeta.Settings.Containers;

namespace Orion.Zeta.ViewModels {
	public class MainViewModel : BaseViewModel, IModifiableGeneralSetting, IModifiableStyleSetting {
		#region Fields
		private readonly SettingRepository _settingRepository;
		private readonly Lazy<ISearchEngine> _searchEngine;
		private readonly Lazy<SettingsService> _settingsService;
		private readonly Lazy<ISearchMethodService> _methodService;
		private readonly Task _initialisationTask;
		private IItem _suggestion;
		private string _expression;
		private bool _isSearching;
		private readonly TimeSpan _delaySearching;
		private readonly Timer _expressionSearchTimer;
		private DateTime _lastTimeStartSearching;
		private readonly Timer _autoRefreshTimer;
		#endregion

		#region Constructor
		public MainViewModel() {
			this.ApplyDefaultStyleAndDesignValue();
			this._settingRepository = new SettingRepository();
			this._searchEngine = new Lazy<ISearchEngine>(() => new SearchEngine());
			this._settingsService = new Lazy<SettingsService>(() => new SettingsService(this._settingRepository));
			this._methodService = new Lazy<ISearchMethodService>(() => new SearchMethodService(new SearchMethodPool(this.SearchEngine), new MefSearchMethodLoader()));
			this.IsSearching = false;
			this.ExpressionAutoCompleteCommand = new RelayCommand(this.OnExpressionAutoCompleteCommand);
			this.ExpressionRunCommand = new RelayCommand(this.OnExpressionRunCommand);
			this.RunCommand = new RelayCommand(this.OnRunCommand);
			this.SelectSuggestionCommand = new RelayCommand(this.OnSelectSuggestionCommand);
			this.OpenSettingCommand = new RelayCommand(this.OnOpenSettingCommand);
			this.Suggestions = new ObservableCollection<IItem>();
			this.Suggestion = null;
			this._expression = string.Empty;
			this._delaySearching = new TimeSpan(0, 0, 0, 0, 250);
			this._expressionSearchTimer = new Timer(this._delaySearching.TotalMilliseconds) { AutoReset = false };
			this._expressionSearchTimer.Elapsed += (sender, args) => {
				this.StartSearching(this.Expression);
			};
			this._autoRefreshTimer = new Timer();
			this._autoRefreshTimer.Elapsed += (sender, args) => {
				this.AutoRefreshSearchEngine();
			};
			this._initialisationTask = this.InitialisationSearchEngineAsync();
			this._isSettingOpen = false;
		}
		#endregion

		#region Functionality
		#region Properties
		#region Commands
		public ICommand ExpressionAutoCompleteCommand { get; set; }

		public ICommand ExpressionRunCommand { get; set; }

		public ICommand RunCommand { get; set; }

		public ICommand SelectSuggestionCommand { get; set; }

		public ICommand OpenSettingCommand { get; set; }
		#endregion

		public string Expression {
			get { return this._expression; }
			set {
				this._expression = value;
				this.OnExpressionUpdated();
			}
		}

		public IItem Suggestion {
			get { return this._suggestion; }
			set {
				this._suggestion = value;
				this.OnPropertyChanged();
			}
		}

		public bool IsSearching {
			get { return this._isSearching; }
			set {
				this._isSearching = value;
				this.OnPropertyChanged();
			}
		}

		public ObservableCollection<IItem> Suggestions { get; set; }

		private ISearchEngine SearchEngine => this._searchEngine.Value;

		private ISettingsService SettingsService => this._settingsService.Value;

		private ISearchMethodService SearchMethodService => this._methodService.Value;

		#endregion

		#region Events
		public event EventHandler OnAutoComplete;
		public event EventHandler OnProgramStart;
		public event EventHandler OnSearchFinished;
		#endregion

		#region Initialisation
		private void InitialisationSearchEngine() {
			this.SettingsService.RegisterGlobal(new GeneralSettingContainer(new ApplicationSettingService(this.SettingsService), this));
			this.SettingsService.RegisterGlobal(new StyleSettingContainer(new ApplicationSettingService(this.SettingsService), this));
			this.SearchMethodService.RegisterSearchMethods(this.SettingsService);
			this.SearchMethodService.RegisterSettings(this.SettingsService);
		}

		private async Task InitialisationSearchEngineAsync() {
			await Task.Run(() => this.InitialisationSearchEngine());
		}
		#endregion

		#region Commands
		private async void OnOpenSettingCommand() {
			this._isSettingOpen = true;
			this.OnPropertyChanged("IsHideWhenLostFocus");
			await this._initialisationTask;
			var settingWindow = new SettingWindow(this.SettingsService, this.SearchMethodService);
			settingWindow.Closed += async (sender, args) => {
				this.SearchMethodService.ManageMethodsBySetting(this.SettingsService);
				await this.SettingsService.SaveChangesAsync();
				this._isSettingOpen = false;
			};
			settingWindow.Show();
		}

		private void OnSelectSuggestionCommand(object obj) {
			var item = obj as IItem;
			if (item != null) {
				this.Suggestion = item;
			}
		}

		private void OnRunCommand(object obj) {
			var item = obj as IItem;
			if (item != null) {
				if (item.IsValid()) {
					item.Execute.Start();
					this.Suggestions.Clear();
					this.OnProgramStart?.Invoke(this, new EventArgs());
				}
			}
		}

		private void OnExpressionRunCommand() {
			if (this._suggestion == null || !this._suggestion.IsValid()) return;
			this._suggestion.Execute.Start();
			this.OnProgramStart?.Invoke(this, new EventArgs());
		}

		private void OnExpressionAutoCompleteCommand() {
			if (this.Suggestion == null || this.Expression.Equals(this.Suggestion.Value)) {
				return;
			}
			this._expression = this.Suggestion.Value;
			this.OnPropertyChanged("Expression");
			this.Suggestions.Clear();
			this.OnAutoComplete?.Invoke(this, new EventArgs());
		}
		#endregion

		#region Search

		private void AutoRefreshSearchEngine() {
			this.SearchEngine.RefreshCache();
		}

		private void OnExpressionUpdated() {
			if (this._expressionSearchTimer.Enabled) {
				this._expressionSearchTimer.Stop();
				this._expressionSearchTimer.Start();
			}
			else {
				this._expressionSearchTimer.Start();
			}
			this.Suggestion = null;
		}

		private void StartSearching(string expression) {
			if (String.IsNullOrEmpty(expression)) {
				if (this.Suggestions.Any()) {
					Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
						this.Suggestions.Clear();
						this.Suggestion = null;
					}));
				}
				return;
			}
			this._lastTimeStartSearching = DateTime.Now;
			this.IsSearching = true;
			if (!this._initialisationTask.IsCompleted) {
				this._initialisationTask.ContinueWith(
					(task =>
						this.SearchEngine.Search(expression)
							.ContinueWith(t => this.SearchingCallback(t.Result, this._lastTimeStartSearching))));
			}
			else {
				this.SearchEngine.Search(expression)
					.ContinueWith(t => this.SearchingCallback(t.Result, this._lastTimeStartSearching));
			}
		}

		private void SearchingCallback(IEnumerable<IItem> suggestions, DateTime timeStart) {
			if (timeStart != this._lastTimeStartSearching) {
				return;
			}
			var sortedList = suggestions.ToList();
			sortedList.Sort((item1, item2) => item1.Rank < item2.Rank ? -1 : item1.Rank == item2.Rank ? 0 : 1);
			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
				this.Suggestions.Clear();
				if (sortedList.Count() > 1) {
					foreach (var suggestion in sortedList) {
						this.Suggestions.Add(suggestion);
					}
				}
				var best = sortedList.FirstOrDefault();
				this.Suggestion = best;
				this.IsSearching = false;
				this.OnSearchFinished?.Invoke(this, new EventArgs());
			}));
		}
		#endregion
		#endregion

		#region Style & Design
		#region IModifiableGeneralSetting
		private bool _isHideWhenLostFocus;
		private bool _isAlwaysOnTop;
		private bool _startOnBoot;
		private bool _isSettingOpen;

		public bool IsHideWhenLostFocus {
			get {
				return !this._isSettingOpen && this._isHideWhenLostFocus;
			}
			set {
				this._isHideWhenLostFocus = value;
				this.OnPropertyChanged();
			}
		}

		public bool IsAlwaysOnTop {
			get { return this._isAlwaysOnTop; }
			set {
				this._isAlwaysOnTop = value;
				this.OnPropertyChanged();
			}
		}

		public bool StartOnBoot {
			get { return this._startOnBoot; }
			set {
				this._startOnBoot = value;
				this.ToogleStartOnBoot(value);
			}
		}

		private void ToogleStartOnBoot(bool value) {
			var app = Application.Current as App;
			app?.ToggleStartOnBoot(value);
		}

		public void EnabledAutoRefresh(int interval) {
			if (interval <= 0) {
				return;
			}
			this._autoRefreshTimer.Stop();
			this._autoRefreshTimer.Interval = interval * 60000;
			this._autoRefreshTimer.Start();
		}

		public void DisabledAutoRefresh() {
			this._autoRefreshTimer.Stop();
		}

		#endregion

		private void ApplyDefaultStyleAndDesignValue() {
			this.UseNoneWindowStyle = false;
			this.Width = 800;
		}

		#region IModifiableStyleSetting
		private bool _useNoneWindowStyle;
		private double _width;

		/// <summary>
		/// Get or set WindowStyle None or not.
		/// </summary>
		public bool UseNoneWindowStyle {
			get { return this._useNoneWindowStyle; }
			set {
				this._useNoneWindowStyle = value;
				this.OnPropertyChanged();
			}
		}

		/// <summary>
		/// Get or set width of application
		/// </summary>
		public double Width {
			get { return this._width; }
			set {
				this._width = value;
				this.OnPropertyChanged();
			}
		}
		#endregion
		#endregion
	}

	public interface IModifiableStyleSetting {
		bool UseNoneWindowStyle { get; set; }

		double Width { get; set; }
    }

	public interface IModifiableGeneralSetting {
		bool IsHideWhenLostFocus { get; set; }
		bool IsAlwaysOnTop { get; set; }
		bool StartOnBoot { get; set; }
		void EnabledAutoRefresh(int interval);
		void DisabledAutoRefresh();
	}
}