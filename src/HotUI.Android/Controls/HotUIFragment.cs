﻿using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using AView = Android.Views.View;

namespace HotUI.Android.Controls
{
    public class HotUIFragment : Fragment
    {
        private readonly View view;
        public string Title { get; }

        public HotUIFragment(View view)
        {
            this.view = view;
            this.Title = view?.GetEnvironment<string>(EnvironmentKeys.View.Title) ?? "";
        }
        AView currentBuiltView;
        public override AView OnCreateView(LayoutInflater inflater,
            ViewGroup container,
            Bundle savedInstanceState) => currentBuiltView = view.ToView(false);
        public override void OnDestroy()
        {
            if (view != null)
            {
                view.ViewHandler = null;
            }
            if (currentBuiltView != null)
            {
                currentBuiltView?.Dispose();
                currentBuiltView = null;
            }
            base.OnDestroy();
            this.Dispose();
        }
    }
}
