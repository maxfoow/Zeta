﻿using System.Drawing;

namespace Orion.Zeta.Core.SearchMethods.Shared {
	public class Item : IItem {
		public Item(string value, Icon icon) {
			this.Value = value;
			this.Icon = icon;
		}

		public Item() {
		}

		public string Value { get; set; }

		public string DisplayName { get; set; }

		public Icon Icon { get; set; }

		public IExecute Execute { get; set; }
		public bool IsValid() {
			return this.Execute != null;
		}
	}
}