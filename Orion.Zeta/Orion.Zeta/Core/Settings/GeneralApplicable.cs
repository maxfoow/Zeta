﻿using System;
using Orion.Zeta.Methods.Dev;
using Orion.Zeta.Settings.Models;
using Orion.Zeta.ViewModels;

namespace Orion.Zeta.Core.Settings {
    public class GeneralApplicable : IModifiableSettings {
        private readonly IModifiableGeneralSetting _modifiableGeneralSetting;

        public GeneralApplicable(IModifiableGeneralSetting modifiableGeneralSetting) {
            this._modifiableGeneralSetting = modifiableGeneralSetting;
        }

        public void ApplyChanges(object item) {
            var generalSetting = item as GeneralModel;
            if (generalSetting == null)
                throw new NullReferenceException("bad item type");
            this._modifiableGeneralSetting.IsHideWhenLostFocus = generalSetting.IsHideWhenLostFocus;
            this._modifiableGeneralSetting.IsAlwaysOnTop = generalSetting.IsAlwaysOnTop;
            if (generalSetting.IsAutoRefreshEnbabled)
                this._modifiableGeneralSetting.EnabledAutoRefresh(generalSetting.AutoRefresh);
            else
                this._modifiableGeneralSetting.DisabledAutoRefresh();
            this._modifiableGeneralSetting.StartOnBoot = generalSetting.IsStartOnBoot;
        }
    }
}