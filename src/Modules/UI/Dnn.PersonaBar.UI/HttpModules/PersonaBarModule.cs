﻿#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Web;
using Dnn.PersonaBar.Library.AppEvents;
using Dnn.PersonaBar.Library.Common;
using DotNetNuke.Application;
using DotNetNuke.Collections;
using DotNetNuke.Instrumentation;
using DotNetNuke.UI.Skins.EventListeners;

namespace Dnn.PersonaBar.UI.HttpModules
{
    public class PersonaBarModule : IHttpModule
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersonaBarModule));

        private static readonly object LockAppStarted = new object();
        private static bool _hasAppStarted = false;
        
        public void Init(HttpApplication application)
        {
            application.BeginRequest += OnBeginRequest;

            if (_hasAppStarted)
            {
                return;
            }
            
            lock (LockAppStarted)
            {
                if (_hasAppStarted)
                {
                    return;
                }

                ApplicationStart();
                _hasAppStarted = true;
            }
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;

            if (context.Request.Url.AbsolutePath.Split('/')[1].Equals("admin2", StringComparison.InvariantCultureIgnoreCase))
            {                
                // TODO: why Server.Transfer gives a 404?
                //context.Server.Transfer("~/DesktopModules/Admin/dnn.PersonaBar/StandaloneContainer.aspx", false);
                context.Server.TransferRequest("~/DesktopModules/Admin/dnn.PersonaBar/StandaloneContainer.aspx", false);
            }
        }

        public void Dispose()
        {
            EventsController.Instance.ApplicationEndEvent();
        }

        private void ApplicationStart()
        {
            EventsController.Instance.ApplicationStartEvent();

            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinInit, OnSkinInit));
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinLoad, OnSkinLoad));
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinPreRender, OnSkinPreRender));
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinUnLoad, OnSkinUnLoad));
        }

        private void OnSkinInit(object sender, SkinEventArgs e)
        {
            IocUtil.GetInstanceContracts<ISkinEvents>().ForEach(instance =>
            {
                try
                {
                    instance.Init(e);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("{0}.Init threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName, ex.Message, ex.StackTrace);
                }
            });
        }

        private void OnSkinLoad(object sender, SkinEventArgs e)
        {
            IocUtil.GetInstanceContracts<ISkinEvents>().ForEach(instance =>
            {
                try
                {
                    instance.Load(e);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("{0}.Load threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName, ex.Message, ex.StackTrace);
                }
            });
        }
        private void OnSkinPreRender(object sender, SkinEventArgs e)
        {
            IocUtil.GetInstanceContracts<ISkinEvents>().ForEach(instance =>
            {
                try
                {
                    instance.PreRender(e);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("{0}.PreRender threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName, ex.Message, ex.StackTrace);
                }
            });
        }

        private void OnSkinUnLoad(object sender, SkinEventArgs e)
        {
            IocUtil.GetInstanceContracts<ISkinEvents>().ForEach(instance =>
            {
                try
                {
                    instance.UnLoad(e);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("{0}.UnLoad threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName, ex.Message, ex.StackTrace);
                }
            });
        }
    }
}