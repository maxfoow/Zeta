﻿using System.Collections.Generic;
using NUnit.Framework;
using Orion.Zeta.Methods.ApplicationSearch;

namespace Orin.Zeta.Core.Tests.SearchMethods.ApplicationSearch {
	[TestFixture]
	public class ApplicationSearchMethodTests {
		private ApplicationSearchMethod _applicationSearchMethod;
		private List<string> _patterns;
		private const string PathToRegister = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs";

		[SetUp]
		public void Setup() {
			this._applicationSearchMethod = new ApplicationSearchMethod();
			this._patterns = new List<string> {
				"*.exe",
				"*.lnk"
			};
		}

		[Test]
		public void GivenApplicationSearch_WhenIsMatchingWithGoodExpression_ThenShouldReturnTrue([Values("Cal", "Control Pa", "vs201", "test@test", "game-of year")] string expression) {
			var matching = this._applicationSearchMethod.IsMatching(expression);

			Assert.IsTrue(matching);
		}

		[Test]
		public void GivenApplicationSearch_WhenIsMachtingWithBadExpression_ThenShouldReturnFalse([Values("@application", "/home")] string expression) {
			var matching = this._applicationSearchMethod.IsMatching(expression);

			Assert.IsFalse(matching);			
		}

		[Test]
		public void GivenApplicationSearch_WhenRegisterPath_ThenShouldBeRegistered() {
			this._applicationSearchMethod.RegisterPath(PathToRegister, this._patterns);

			Assert.IsTrue(this._applicationSearchMethod.IsRegistered(PathToRegister));
		}

		[Test]
		public void GivenApplicationSearchWithRegisteredPath_WhenUnregisterPath_ThenShouldUnRegisterTheGivenPath() {
			this._applicationSearchMethod.RegisterPath(PathToRegister, this._patterns);

			this._applicationSearchMethod.UnregisterPath(PathToRegister);

			Assert.IsFalse(this._applicationSearchMethod.IsRegistered(PathToRegister));
		}
	}
}