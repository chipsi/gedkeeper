﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2017 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using GKCommon.IoC;
using GKCore.Interfaces;
using GKUI.Contracts;

namespace GKCore
{
    /// <summary>
    /// A prototype of the future global controller of UI for the isolation
    /// of presentation from logic and data model (based on IoC).
    /// </summary>
    public static class AppHub
    {
        private static readonly IocContainer fIocContainer;

        private static IStdDialogs fStdDialogs;
        private static BaseController fBaseController;
        private static IPathReplacer fPathReplacer;
        private static INamesTable fNamesTable;
        private static IUtilities fUtilities;
        private static IHost fMainWindow;
        private static IProgressController fProgressController;


        public static IocContainer Container
        {
            get { return fIocContainer; }
        }

        #region Direct instances

        public static IHost MainWindow
        {
            get { return fMainWindow; }
            set { fMainWindow = value; }
        }

        public static BaseController BaseController
        {
            get {
                if (fBaseController == null) {
                    fBaseController = new BaseController();
                }
                return fBaseController;
            }
        }

        public static IPathReplacer PathReplacer
        {
            get {
                if (fPathReplacer == null) {
                    fPathReplacer = new PathReplacer();
                }
                return fPathReplacer;
            }
        }

        public static INamesTable NamesTable
        {
            get {
                if (fNamesTable == null) {
                    fNamesTable = new NamesTable();
                }
                return fNamesTable;
            }
        }

        #endregion

        #region UI-dependent instances

        public static IProgressController Progress
        {
            get {
                if (fProgressController == null) {
                    fProgressController = fIocContainer.Resolve<IProgressController>();
                }
                return fProgressController;
            }
        }

        public static IStdDialogs StdDialogs
        {
            get {
                if (fStdDialogs == null) {
                    fStdDialogs = fIocContainer.Resolve<IStdDialogs>();
                }
                return fStdDialogs;
            }
        }

        public static IUtilities Utilities
        {
            get {
                if (fUtilities == null) {
                    fUtilities = fIocContainer.Resolve<IUtilities>();
                }
                return fUtilities;
            }
        }

        #endregion

        static AppHub()
        {
            fIocContainer = new IocContainer();
        }

        public static void InitHost()
        {
            
        }

        public static void DoneHost()
        {
            
        }
    }
}
