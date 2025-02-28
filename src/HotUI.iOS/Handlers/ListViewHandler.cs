﻿using System;
using Foundation;
using HotUI.iOS.Controls;
using UIKit;

// ReSharper disable ClassNeverInstantiated.Global

namespace HotUI.iOS.Handlers
{
    public class ListViewHandler : AbstractHandler<ListView, HUITableView>
    {
        public static readonly PropertyMapper<ListView> Mapper = new PropertyMapper<ListView>(ViewHandler.Mapper)
        {
            ["ListView"] = MapListViewProperty,
            [nameof(ListView.ReloadData)] = MapReloadData
        };
        
        public ListViewHandler() : base(Mapper)
        {

        }
        
        protected override HUITableView CreateView()
        {
            return new HUITableView();
        }

        public override void Remove(View view)
        {
            TypedNativeView.ListView = null;
            base.Remove(view);
        }

        public static void MapListViewProperty(IViewHandler viewHandler, ListView virtualView)
        {
            var nativeView = (HUITableView) viewHandler.NativeView;
            nativeView.ListView = virtualView;
            nativeView.SizeToFit();
        }

        public static void MapReloadData(IViewHandler viewHandler, ListView virtualView)
        {
            var nativeView = (HUITableView)viewHandler.NativeView;
            nativeView?.ReloadData();
        }
    }
}