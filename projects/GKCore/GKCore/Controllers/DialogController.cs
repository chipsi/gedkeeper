﻿/*
 *  "GEDKeeper", the personal genealogical database editor.
 *  Copyright (C) 2009-2018 by Sergey V. Zhdanovskih.
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
using System.Collections.Generic;
using GKCore.Interfaces;

namespace GKCore.Controllers
{
    public interface IControlHandler
    {
    }

    public abstract class ControlHandler<T, TThis> : IControlHandler where TThis : ControlHandler<T, TThis>
    {
        protected T fControl;

        public virtual T Control
        {
            get { return fControl; }
        }

        protected ControlHandler(T control)
        {
            this.fControl = control;
        }
    }

    public interface IComboBoxHandler : IControlHandler
    {
        bool Enabled { get; set; }
        int SelectedIndex { get; set; }
        string Text { get; set; }
    }

    public interface ITextBoxHandler : IControlHandler
    {
        bool Enabled { get; set; }
        string Text { get; set; }
    }

    public sealed class ControlsManager
    {
        private static readonly Dictionary<Type, Type> fHandlerTypes = new Dictionary<Type, Type>();

        private readonly Dictionary<object, IControlHandler> fHandlers = new Dictionary<object, IControlHandler>();

        public IControlHandler GetControlHandler(object control)
        {
            IControlHandler handler;
            if (!fHandlers.TryGetValue(control, out handler)) {
                Type controlType = control.GetType();
                Type handlerType;
                if (fHandlerTypes.TryGetValue(controlType, out handlerType)) {
                    handler = (IControlHandler)Activator.CreateInstance(handlerType, control);
                } else {
                    throw new Exception("handler type not found");
                }
            }
            return handler;
        }

        public static void RegisterHandlerType(Type controlType, Type handlerType)
        {
            if (fHandlerTypes.ContainsKey(controlType)) {
                fHandlerTypes.Remove(controlType);
            }
            fHandlerTypes.Add(controlType, handlerType);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class DialogController
    {
        protected IBaseWindow fBase;


        public abstract bool Accept();

        public virtual void Cancel()
        {
            // dummy
        }

        public virtual void Init(IBaseWindow baseWin)
        {
            fBase = baseWin;
        }

        public abstract void UpdateView();
    }
}