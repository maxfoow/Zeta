﻿using System.Collections.Generic;

namespace Orion.Zeta.Core.Settings.SearchMethods {
    public class Directory {
        public Directory() {
            this.Extensions = new List<string>();
        }
        public string Path { get; set; }

        public string SpecialFolder { get; set; }

        public List<string> Extensions { get; set; } 
    }
}