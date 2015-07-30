using System.Collections;
using Android.App;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Widget;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Design.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Modules;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.Design.Modules
{
    public class DesignModule : ModuleBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DesignModule" /> class.
        /// </summary>
        public DesignModule()
            : base(true, priority: BindingModulePriority - 1)
        {
        }

        #endregion

        #region Overrides of ModuleBase

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
            IBindingMemberProvider provider = BindingServiceProvider.MemberProvider;
            //NavigationView
            provider.Register(
                AttachedBindingMember.CreateAutoProperty(AttachedMembersDesign.NavigationView.MenuTemplate,
                    NavigationViewMenuTemplateChanged));

            //TabLayout              
            provider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemsSource.Override<TabLayout>(), TabLayoutItemsSourceChanged));
            provider.Register(AttachedBindingMember.CreateMember(AttachedMembersDesign.TabLayout.SelectedItem,
                    TabLayoutGetSelectedItem, TabLayoutSetSelectedItem, (info, layout, arg3) =>
                    {
                        arg3 = arg3.ToWeakEventListener();
                        layout.TabSelected += arg3.Handle;
                        layout.TabUnselected += arg3.Handle;
                        return WeakActionToken.Create(layout, arg3, (tabLayout, listener) =>
                        {
                            tabLayout.TabSelected -= listener.Handle;
                            tabLayout.TabUnselected -= listener.Handle;
                        });
                    }));

            //TabLayout.Tab
            provider.Register(AttachedBindingMember.CreateNotifiableMember<TabLayout.Tab, string>("Text",
                (info, tab) => tab.Text,
                (info, tab, arg3) =>
                {
                    tab.SetText(arg3);
                    return true;
                }));
            provider.Register(AttachedBindingMember.CreateNotifiableMember<TabLayout.Tab, string>("ContentDescription",
                (info, tab) => tab.ContentDescription,
                (info, tab, arg3) =>
                {
                    tab.SetContentDescription(arg3);
                    return true;
                }));
            provider.Register(AttachedBindingMember.CreateNotifiableMember<TabLayout.Tab, object>("Icon",
                (info, tab) => tab.Icon,
                (info, tab, arg3) =>
                {
                    if (arg3 is int)
                        tab.SetIcon((int)arg3);
                    else
                        tab.SetIcon((Drawable)arg3);
                    return true;
                }));
            provider.Register(AttachedBindingMember.CreateNotifiableMember<TabLayout.Tab, Object>("Tag",
                (info, tab) => tab.Tag,
                (info, tab, arg3) =>
                {
                    tab.SetTag(arg3);
                    return true;
                }));

            //EditText
            provider.Register(AttachedBindingMember.CreateAutoProperty<EditText, string>(BindingErrorProvider.ErrorPropertyName,
                    (text, args) =>
                    {
                        var layout = text.Parent as TextInputLayout;
                        if (layout == null)
                            text.Error = args.NewValue;
                        else
                        {
                            text.Error = null;
                            layout.SetError(args.NewValue);
                        }
                    }, getDefaultValue: (text, info) => text.Error));

            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        protected override void UnloadInternal()
        {
        }

        #endregion

        #region Methods

        private static void TabLayoutSetSelectedItem(IBindingMemberInfo bindingMemberInfo, TabLayout tabLayout, object selectedItem)
        {
            for (int i = 0; i < tabLayout.TabCount; i++)
            {
                var tab = tabLayout.GetTabAt(i);
                if (tab.GetDataContext() == selectedItem)
                {
                    if (tab.Position != tabLayout.SelectedTabPosition)
                        tab.Select();
                    return;
                }
            }
        }

        private static object TabLayoutGetSelectedItem(IBindingMemberInfo bindingMemberInfo, TabLayout tabLayout)
        {
            var p = tabLayout.SelectedTabPosition;
            if (p < 0)
                return null;
            return tabLayout.GetTabAt(p).GetDataContext();
        }

        private static void TabLayoutItemsSourceChanged(TabLayout view, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var generator = view.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator);
            if (generator == null)
            {
                generator = new TabLayoutItemsSourceGenerator(view);
                view.SetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator, generator);
            }
            generator.SetItemsSource(args.NewValue);
        }

        private static void NavigationViewMenuTemplateChanged(NavigationView view,
            AttachedMemberChangedEventArgs<int> args)
        {
            Activity activity = view.Context.GetActivity();
            if (activity != null)
                activity.MenuInflater.Inflate(args.NewValue, view.Menu, view);
        }

        #endregion
    }
}