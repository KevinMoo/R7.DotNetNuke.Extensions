﻿//
// EditPortalModuleBase.cs
//
// Author:
//       Roman M. Yagodin <roman.yagodin@gmail.com>
//
// Copyright (c) 2016
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.UserControls;

namespace DotNetNuke.R7.Entities.Modules
{
    /// <summary>
    /// A base class to build simple edit module controls
    /// </summary>
    public abstract class EditPortalModuleBase<TItem, TKey>: PortalModuleBase
        where TItem: class, new ()
        where TKey: struct
    {
        #region Fields

        protected TKey? ItemId;

        protected readonly string Key;

        protected readonly bool EnableAjax;

        #endregion

        #region Controls

        /// <summary>
        /// The update button.
        /// </summary>
        protected LinkButton ButtonUpdate;

        /// <summary>
        /// The delete button.
        /// </summary>
        protected LinkButton ButtonDelete;

        /// <summary>
        /// The cancel link.
        /// </summary>
        protected HyperLink LinkCancel;

        /// <summary>
        /// The module audit control.
        /// </summary>
        protected ModuleAuditControl ModuleAuditControl;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetNuke.R7.EditPortalModuleBase{TItem,TKey}"/> class.
        /// </summary>
        /// <param name="key">Key.</param>
        protected EditPortalModuleBase (string key): this (key, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetNuke.R7.EditPortalModuleBase{TItem,TKey}"/> class.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="enableAjax">If set to 'true', module will try to register AJAX script manager, if AJAX is installed.</param>
        protected EditPortalModuleBase (string key, bool enableAjax)
        {
            Key = key;
            EnableAjax = enableAjax;
        }

        /// <summary>
        /// Handles Page_Init event
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnInit (EventArgs e)
        {
            base.OnInit (e);

            InitControls ();

            // wireup handlers
            ButtonUpdate.Click += OnButtonUpdateClick;
            ButtonDelete.Click += OnButtonDeleteClick;

            // set url for Cancel link
            LinkCancel.NavigateUrl = Globals.NavigateURL ();

            // add confirmation dialog to delete button
            ButtonDelete.Attributes.Add ("onClick", "javascript:return confirm('" 
                + Localization.GetString ("DeleteItem") + "');");
        }

        /// <summary>
        /// Init controls, required by <see cref="DotNetNuke.R7.EditModuleBase{TItem,TKey}"/>
        /// Provides interface to implement OnInitControls in child classes.
        /// </summary>
        /// <param name="buttonUpdate">Update linkbutton.</param>
        /// <param name="buttonDelete">Delete linkbutton.</param>
        /// <param name="linkCancel">Cancel hyperlink.</param>
        /// <param name="moduleAuditControl">Optional module audit control.</param>
        protected void InitControls (LinkButton buttonUpdate, LinkButton buttonDelete, HyperLink linkCancel, ModuleAuditControl moduleAuditControl = null)
        {
            ButtonUpdate = buttonUpdate;
            ButtonDelete = buttonDelete;
            LinkCancel = linkCancel;
            ModuleAuditControl = moduleAuditControl;
        }

        /// <summary>
        /// Handles the Page_Load event.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnLoad (EventArgs e)
        {
            base.OnLoad (e);

            if (EnableAjax && DotNetNuke.Framework.AJAX.IsInstalled ()) {
                DotNetNuke.Framework.AJAX.RegisterScriptManager ();
            }

            try
            {
                // parse querystring parameters
                ItemId = TypeUtils.ParseToNullable<TKey> (Request.QueryString [Key]);

                if (!IsPostBack)
                {
                    // load the data into the control the first time we hit this page

                    // check we have an item to lookup
                    if (ItemId != null)
                    {
                        // load the item
                        var item = GetItem (ItemId.Value);

                        if (item != null && CanEditItem (item))
                        {
                            ButtonDelete.Visible = CanDeleteItem (item);
                            LoadItem (item);
                        }
                        else
                            Response.Redirect (Globals.NavigateURL (), true);
                    }
                    else
                    {
                        ButtonDelete.Visible = false;
                        if (ModuleAuditControl != null)
                        {
                            ModuleAuditControl.Visible = false;
                        }

                        LoadNewItem ();
                    }
                }
                else
                {
                    PostBack ();
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException (this, ex);
            }
        }

        /// <summary>
        /// Handles item update button click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnButtonUpdateClick (object sender, EventArgs e)
        {
            try
            {
                // create new or get existing item
                var item = (ItemId == null) ? new TItem () : GetItem (ItemId.Value);

                BeforeUpdateItem (item);

                if (ItemId == null)
                {
                    AddItem (item);
                }
                else
                {
                    UpdateItem (item);
                }

                AfterUpdateItem (item);

                // synchronize module
                ModuleController.SynchronizeModule (ModuleId);

                Response.Redirect (Globals.NavigateURL (), true);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException (this, ex);
            }
        }

        /// <summary>
        /// Handles item delete button click.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnButtonDeleteClick (object sender, EventArgs e)
        {
            try
            {
                if (ItemId.HasValue)
                {
                    var item = GetItem (ItemId.Value);
                    if (item != null && CanDeleteItem (item))
                    {
                        DeleteItem (item);
                        Response.Redirect (Globals.NavigateURL (), true);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException (this, ex);
            }
        }


        #region Repository extension points

        protected abstract TItem GetItem (TKey itemId);

        protected abstract TKey AddItem (TItem item);

        protected abstract void UpdateItem (TItem item);

        protected abstract void DeleteItem (TItem item);

        #endregion

        #region Other extension points

        /// <summary>
        /// Implement to pass references to the required module controls
        /// </summary>
        /// <example>
        /// <code>
        /// protected override void OnInitControls () 
        /// {
        ///    InitControls (buttonUpdate, buttonDelete, linkCancel);
        /// }
        /// </code>
        /// </example>
        protected abstract void InitControls ();

        /// <summary>
        /// Implement to provide item to form controls binding code here.
        /// </summary>
        /// <param name="item">Item.</param>
        protected abstract void LoadItem (TItem item);

        /// <summary>
        /// Override to provide custom code which should be called for new items.
        /// </summary>
        protected virtual void LoadNewItem ()
        {}

        /// <summary>
        /// Override to provide code which should be called on Page_Load then (IsPostBack == true) here
        /// </summary>
        protected virtual void PostBack ()
        {}

        /// <summary>
        /// Implement to provide code to fill item from form controls here.
        /// </summary>
        /// <param name="item">Item.</param>
        protected abstract void BeforeUpdateItem (TItem item);

        /// <summary>
        /// Implement to provide code which will be called 
        /// after item update in the DB
        /// </summary>
        /// <param name="item">Item.</param>
        protected virtual void AfterUpdateItem (TItem item)
        {}

        /// <summary>
        /// Override to define edit permission checks here.
        /// </summary>
        /// <returns><c>true</c> if the specified item can be edited; otherwise, <c>false</c>.</returns>
        /// <param name="item">Item.</param>
        protected virtual bool CanEditItem (TItem item)
        {
            return true;
        }

        /// <summary>
        /// Override to define delete permission checks here.
        /// </summary>
        /// <returns><c>true</c> if the specified item can be deleted; otherwise, <c>false</c>.</returns>
        /// <param name="item">Item.</param>
        protected virtual bool CanDeleteItem (TItem item)
        {
            return true;
        }

        #endregion
    }
}

