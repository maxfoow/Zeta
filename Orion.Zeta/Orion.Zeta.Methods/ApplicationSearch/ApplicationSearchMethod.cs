﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Orion.Zeta.Methods.Dev;
using Orion.Zeta.Methods.Dev.Shared;
using Orion.Zeta.Methods.Dev.Shared.Implementations;

namespace Orion.Zeta.Methods.ApplicationSearch {
	public class ApplicationSearchMethod : ISearchMethodAsync {
		private readonly List<ApplicationsContainer> _applications;
		private readonly IFileSystemSearch _fileSystemSearch;
		private readonly Regex _rgx;
		private Task _initialisationTask;

		public ApplicationSearchMethod() {
			this._fileSystemSearch = new FileSystemSearch();
			this._applications = new List<ApplicationsContainer>();
			this._rgx = new Regex("^(?!.*/|:|@).*$");
		}

		public bool IsMatching(string expression) {
			return this._rgx.IsMatch(expression);
		}

		public void Initialisation() {
			foreach (var applicationsContainer in this._applications) {
				applicationsContainer.BuildCache();
			}
		}

		public IEnumerable<IItem> Search(string expression) {
			var items = new List<IItem>();
			foreach (var container in this._applications) {
				items = container.Search(expression).Concat(items).ToList();
			}
			return items;
		}

	    public void RefreshCache() {
	        this._initialisationTask = this.InitialisationAsync();
	    }

	    public void RegisterPath(string path, IEnumerable<string> patterns) {
			this._applications.Add(new ApplicationsContainer(path, patterns, this._fileSystemSearch, false));
		}

		public void UnregisterPath(string path) {
			var applicationsContainer = this._applications.FirstOrDefault(a => a.Path.Equals(path));
			if (applicationsContainer == null) {
				throw new PathUnRegisteredException(path);
			}
			applicationsContainer.ClearCache();
			this._applications.Remove(applicationsContainer);
		}

		public bool IsRegistered(string path) {
			return this._applications.Any(a => a.Path.Equals(path));
		}

		public Task InitialisationAsync() {
			this._initialisationTask = Task.Run(() => {
				this.Initialisation();
			});
			return this._initialisationTask;
		}

		public async Task<IEnumerable<IItem>> SearchAsync(string expression) {
			if (!this._initialisationTask.IsCompleted) {
				await this._initialisationTask;
			}
			return await Task.Run(() => this.Search(expression));
		}
	}
}