﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orion.Zeta.Methods.Dev.Shared;

namespace Orion.Zeta.Methods.ExplorerSearch {
	public class ExpressionPath {

		private readonly string _expression;
		private readonly IFileSystemSearch _fileSystemSearch;

		public ExpressionPath(string expression, IFileSystemSearch fileSystemSearch) {
			this._expression = expression;
			this._fileSystemSearch = fileSystemSearch;
		}

		public IEnumerable<PossibilityPath> FindPossibilities() {
			return this.GeneratePossibilities(PathHelper.ConvertToWinPath(this._expression));
		}

		private IEnumerable<PossibilityPath> GeneratePossibilities(IEnumerable<string> paths) {
			var sortStrategy = this.IsBaseDirectory(paths) ? (IComparer<PossibilityPath>) new SortPathWindowsStyle() : new SortPathBetterMatchStrategy();
			var results = new CollectionExpressionPath(sortStrategy);
			foreach (var path in paths) {
				var parentDirectoryPath = PathHelper.GetParentDirectory(path);
				var pattern = PathHelper.GetPattern(path);
				pattern += "*";
                IEnumerable<string> files;
                try
                {
			        files = this._fileSystemSearch.GetFiles(parentDirectoryPath, pattern);
			    }
			    catch (DirectoryNotFoundException)
			    {
			        continue;
			    }
				foreach (var file in files) {
					results.Add(new PossibilityPath(file, this._expression, PossibilityPath.PathType.File));
				}
				var directories = this._fileSystemSearch.GetDirectories(parentDirectoryPath, pattern);
				foreach (var directory in directories) {
					/*
					 *  Path because /Example/Zeta/Zeta.
					 *  Match with /Example/Zeta/Zeta
					 *  if /Example/Zeta contains folder name Zeta
					 * */
					if (directory.Length >= path.Length) {
						results.Add(new PossibilityPath(directory, this._expression, PossibilityPath.PathType.Directory));
					}
				}
			}
			results.SortAndRank();
			return results;
		}

		private bool IsBaseDirectory(IEnumerable<string> paths) {
			return paths.All(p => p.EndsWith(@"\"));
		}
	}
}