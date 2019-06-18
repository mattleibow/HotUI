﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HotForms {
	public static class DatabindingExtensions {
		public static void SetValue<T> (this View view, State state, ref T currentValue, T newValue, Action<object> onUpdate, [CallerMemberName] string propertyName = "")
		{
			if (state?.IsBuilding ?? false) {
				var props = state.EndProperty ();
				var propCount = props.Length;
				//This is databound!
				if (propCount > 0) {
					bool isGlobal = propCount > 1;
					if (propCount == 1) {
						var prop = props [0];
						var stateValue = (T)state.GetValue (prop);
						//1 to 1 binding!
						if (EqualityComparer<T>.Default.Equals (stateValue, newValue)) {
							state.BindingState.AddViewProperty (prop, onUpdate);
							Debug.WriteLine ($"Databinding: {view.GetType ()}.{propertyName} to {prop}");
						} else {
							Debug.WriteLine ($"Warning: {propertyName} is using formated Text. For performance reasons, please switch to TextBinding");
							isGlobal = true;
						}
					} else {
						Debug.WriteLine ($"Warning: {propertyName} is using Multiple state Variables. For performance reasons, please switch to TextBinding");
					}

					if (isGlobal) {

						state.BindingState.AddGlobalProperties (props);
					}
				}
			}
			currentValue = newValue;
			onUpdate (newValue);
		}


		public static View DiffUpdate (this View newView, View oldView)
		{
			if (!newView.AreSameType (oldView)) {
				return newView;
			}

			//Yes if one is IContainer, the other is too!
			if (newView is IContainerView newContainer && oldView is IContainerView oldContainer) {
				var newChildren = newContainer.GetChildren ();
				var oldChildren = oldContainer.GetChildren ().ToList () ;
				for (var i = 0; i < Math.Max (newChildren.Count, oldChildren.Count); i++) {
					var n = newChildren.GetViewAtIndex (i);
					var o = oldChildren.GetViewAtIndex (i);
					if(n.AreSameType(o)) {
						Debug.WriteLine ("The controls are the same!");
						DiffUpdate (n, o);
						continue;
					}

					if (i + 1 >= newChildren.Count || i + 1 >= oldChildren.Count) {
						//We are at the end, no point in searching
						continue;
					}
					//Lets see if the next 2 match
					var o1 = oldChildren.GetViewAtIndex (i + 1);
					var n1 = newChildren.GetViewAtIndex (i + 1);
					if (n1.AreSameType (o1)) {
						Debug.WriteLine ("The controls were replaced!");
						//No big deal the control was replaced!
						continue;
					}
					if (n.AreSameType (o1)) {
						//we removed one from the old Children and use the next one

						Debug.WriteLine ("One control was removed");
						DiffUpdate (n, o1);
						oldChildren.RemoveAt (i);
						continue;
					}
					if (n1.AreSameType (o)){
						//The next ones line up, so this was just a new one being inserted!
						//Lets add an empty one to make them line up

						Debug.WriteLine ("One control was added");
						DiffUpdate (n1, o);
						oldChildren.Insert (i, null);
						continue;
					}
					
					//They don't line up. Maybe we check if 2 were inserted? But for now we are just going to say oh well.
					//The view will jsut be recreated for the restof these!
					Debug.WriteLine ("Oh WEll");

				}
			}



			var formsContnrol = oldView.FormsView;
			//This will unhook any events
			oldView.UpdateFromOldView (null);
			//Use the old control.
			newView.UpdateFromOldView (formsContnrol);


			return newView;

		}

		static View GetViewAtIndex(this IReadOnlyList<View> list, int index)
		{
			if (index >= list.Count)
				return null;
			return list [index];
		}



		public static bool AreSameType (this View view, View compareView)
		{
			if (view is FormsView f1 && view is FormsView f2) {
				return f1.FormsViewType == f2.FormsViewType;
			}

			//Add in more edge cases
			return view?.GetType () == compareView?.GetType ();
		}

	}
}
