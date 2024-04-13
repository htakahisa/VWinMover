
// TODO : check https://github.com/MScholtes/VirtualDesktop/blob/master/VirtualDesktop11.cs
// TODO : copy this Program.cs to ProgramX.cs
// TODO : reflesh "VirtualDesktop" namespace on this Program.cs

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;

// set attributes
using VirtualDesktop;
using VWinMover;
using System.Configuration;

// set attributes
//using System.Reflection;
//[assembly: AssemblyTitle("Command line tool to manage virtual desktops")]
//[assembly: AssemblyDescription("Command line tool to manage virtual desktops")]
//[assembly: AssemblyConfiguration("")]
//[assembly: AssemblyCompany("MS")]
//[assembly: AssemblyProduct("VirtualDesktop")]
//[assembly: AssemblyCopyright("� Markus Scholtes 2024")]
//[assembly: AssemblyTrademark("")]
//[assembly: AssemblyCulture("")]
//[assembly: AssemblyVersion("1.17.0.0")]
//[assembly: AssemblyFileVersion("1.17.0.0")]

// Based on http://stackoverflow.com/a/32417530, Windows 10 SDK, github project Grabacr07/VirtualDesktop and own research

namespace VirtualDesktop
{
	#region COM API
	internal static class Guids
	{
		public static readonly Guid CLSID_ImmersiveShell = new Guid("C2F03A33-21F5-47FA-B4BB-156362A2F239");
		public static readonly Guid CLSID_VirtualDesktopManagerInternal = new Guid("C5E0CDCA-7B6E-41B2-9FC4-D93975CC467B");
		public static readonly Guid CLSID_VirtualDesktopManager = new Guid("AA509086-5CA9-4C25-8F95-589D3C07B48A");
		public static readonly Guid CLSID_VirtualDesktopPinnedApps = new Guid("B5A399E7-1C87-46B8-88E9-FC5747B171BD");
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Size
	{
		public int X;
		public int Y;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct Rect
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

	internal enum APPLICATION_VIEW_CLOAK_TYPE : int
	{
		AVCT_NONE = 0,
		AVCT_DEFAULT = 1,
		AVCT_VIRTUAL_DESKTOP = 2
	}

	internal enum APPLICATION_VIEW_COMPATIBILITY_POLICY : int
	{
		AVCP_NONE = 0,
		AVCP_SMALL_SCREEN = 1,
		AVCP_TABLET_SMALL_SCREEN = 2,
		AVCP_VERY_SMALL_SCREEN = 3,
		AVCP_HIGH_SCALE_FACTOR = 4
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
	[Guid("372E1D3B-38D3-42E4-A15B-8AB2B178F513")]
	internal interface IApplicationView
	{
		int SetFocus();
		int SwitchTo();
		int TryInvokeBack(IntPtr /* IAsyncCallback* */ callback);
		int GetThumbnailWindow(out IntPtr hwnd);
		int GetMonitor(out IntPtr /* IImmersiveMonitor */ immersiveMonitor);
		int GetVisibility(out int visibility);
		int SetCloak(APPLICATION_VIEW_CLOAK_TYPE cloakType, int unknown);
		int GetPosition(ref Guid guid /* GUID for IApplicationViewPosition */, out IntPtr /* IApplicationViewPosition** */ position);
		int SetPosition(ref IntPtr /* IApplicationViewPosition* */ position);
		int InsertAfterWindow(IntPtr hwnd);
		int GetExtendedFramePosition(out Rect rect);
		int GetAppUserModelId([MarshalAs(UnmanagedType.LPWStr)] out string id);
		int SetAppUserModelId(string id);
		int IsEqualByAppUserModelId(string id, out int result);
		int GetViewState(out uint state);
		int SetViewState(uint state);
		int GetNeediness(out int neediness);
		int GetLastActivationTimestamp(out ulong timestamp);
		int SetLastActivationTimestamp(ulong timestamp);
		int GetVirtualDesktopId(out Guid guid);
		int SetVirtualDesktopId(ref Guid guid);
		int GetShowInSwitchers(out int flag);
		int SetShowInSwitchers(int flag);
		int GetScaleFactor(out int factor);
		int CanReceiveInput(out bool canReceiveInput);
		int GetCompatibilityPolicyType(out APPLICATION_VIEW_COMPATIBILITY_POLICY flags);
		int SetCompatibilityPolicyType(APPLICATION_VIEW_COMPATIBILITY_POLICY flags);
		int GetSizeConstraints(IntPtr /* IImmersiveMonitor* */ monitor, out Size size1, out Size size2);
		int GetSizeConstraintsForDpi(uint uint1, out Size size1, out Size size2);
		int SetSizeConstraintsForDpi(ref uint uint1, ref Size size1, ref Size size2);
		int OnMinSizePreferencesUpdated(IntPtr hwnd);
		int ApplyOperation(IntPtr /* IApplicationViewOperation* */ operation);
		int IsTray(out bool isTray);
		int IsInHighZOrderBand(out bool isInHighZOrderBand);
		int IsSplashScreenPresented(out bool isSplashScreenPresented);
		int Flash();
		int GetRootSwitchableOwner(out IApplicationView rootSwitchableOwner);
		int EnumerateOwnershipTree(out IObjectArray ownershipTree);
		int GetEnterpriseId([MarshalAs(UnmanagedType.LPWStr)] out string enterpriseId);
		int IsMirrored(out bool isMirrored);
		int Unknown1(out int unknown);
		int Unknown2(out int unknown);
		int Unknown3(out int unknown);
		int Unknown4(out int unknown);
		int Unknown5(out int unknown);
		int Unknown6(int unknown);
		int Unknown7();
		int Unknown8(out int unknown);
		int Unknown9(int unknown);
		int Unknown10(int unknownX, int unknownY);
		int Unknown11(int unknown);
		int Unknown12(out Size size1);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("1841C6D7-4F9D-42C0-AF41-8747538F10E5")]
	internal interface IApplicationViewCollection
	{
		int GetViews(out IObjectArray array);
		int GetViewsByZOrder(out IObjectArray array);
		int GetViewsByAppUserModelId(string id, out IObjectArray array);
		int GetViewForHwnd(IntPtr hwnd, out IApplicationView view);
		int GetViewForApplication(object application, out IApplicationView view);
		int GetViewForAppUserModelId(string id, out IApplicationView view);
		int GetViewInFocus(out IntPtr view);
		int Unknown1(out IntPtr view);
		void RefreshCollection();
		int RegisterForApplicationViewChanges(object listener, out int cookie);
		int UnregisterForApplicationViewChanges(int cookie);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("3F07F4BE-B107-441A-AF0F-39D82529072C")]
	internal interface IVirtualDesktop
	{
		bool IsViewVisible(IApplicationView view);
		Guid GetId();
		[return: MarshalAs(UnmanagedType.HString)]
		string GetName();
		[return: MarshalAs(UnmanagedType.HString)]
		string GetWallpaperPath();
		bool IsRemote();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("53F5CA0B-158F-4124-900C-057158060B27")]
	internal interface IVirtualDesktopManagerInternal
	{
		int GetCount();
		void MoveViewToDesktop(IApplicationView view, IVirtualDesktop desktop);
		bool CanViewMoveDesktops(IApplicationView view);
		IVirtualDesktop GetCurrentDesktop();
		void GetDesktops(out IObjectArray desktops);
		[PreserveSig]
		int GetAdjacentDesktop(IVirtualDesktop from, int direction, out IVirtualDesktop desktop);
		void SwitchDesktop(IVirtualDesktop desktop);
		//		void SwitchDesktopAndMoveForegroundView(IVirtualDesktop desktop);
		IVirtualDesktop CreateDesktop();
		void MoveDesktop(IVirtualDesktop desktop, int nIndex);
		void RemoveDesktop(IVirtualDesktop desktop, IVirtualDesktop fallback);
		IVirtualDesktop FindDesktop(ref Guid desktopid);
		void GetDesktopSwitchIncludeExcludeViews(IVirtualDesktop desktop, out IObjectArray unknown1, out IObjectArray unknown2);
		void SetDesktopName(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string name);
		void SetDesktopWallpaper(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string path);
		void UpdateWallpaperPathForAllDesktops([MarshalAs(UnmanagedType.HString)] string path);
		void CopyDesktopState(IApplicationView pView0, IApplicationView pView1);
		void CreateRemoteDesktop([MarshalAs(UnmanagedType.HString)] string path, out IVirtualDesktop desktop);
		void SwitchRemoteDesktop(IVirtualDesktop desktop, IntPtr switchtype);
		void SwitchDesktopWithAnimation(IVirtualDesktop desktop);
		void GetLastActiveDesktop(out IVirtualDesktop desktop);
		void WaitForAnimationToComplete();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("A5CD92FF-29BE-454C-8D04-D82879FB3F1B")]
	internal interface IVirtualDesktopManager
	{
		bool IsWindowOnCurrentVirtualDesktop(IntPtr topLevelWindow);
		Guid GetWindowDesktopId(IntPtr topLevelWindow);
		void MoveWindowToDesktop(IntPtr topLevelWindow, ref Guid desktopId);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("4CE81583-1E4C-4632-A621-07A53543148F")]
	internal interface IVirtualDesktopPinnedApps
	{
		bool IsAppIdPinned(string appId);
		void PinAppID(string appId);
		void UnpinAppID(string appId);
		bool IsViewPinned(IApplicationView applicationView);
		void PinView(IApplicationView applicationView);
		void UnpinView(IApplicationView applicationView);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9")]
	internal interface IObjectArray
	{
		void GetCount(out int count);
		void GetAt(int index, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object obj);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
	internal interface IServiceProvider10
	{
		[return: MarshalAs(UnmanagedType.IUnknown)]
		object QueryService(ref Guid service, ref Guid riid);
	}
	#endregion

	#region COM wrapper
	internal static class DesktopManager
	{
		static DesktopManager()
		{
			var shell = (IServiceProvider10)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell));
			VirtualDesktopManagerInternal = (IVirtualDesktopManagerInternal)shell.QueryService(Guids.CLSID_VirtualDesktopManagerInternal, typeof(IVirtualDesktopManagerInternal).GUID);
			VirtualDesktopManager = (IVirtualDesktopManager)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_VirtualDesktopManager));
			ApplicationViewCollection = (IApplicationViewCollection)shell.QueryService(typeof(IApplicationViewCollection).GUID, typeof(IApplicationViewCollection).GUID);
			VirtualDesktopPinnedApps = (IVirtualDesktopPinnedApps)shell.QueryService(Guids.CLSID_VirtualDesktopPinnedApps, typeof(IVirtualDesktopPinnedApps).GUID);
		}

		internal static IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;
		internal static IVirtualDesktopManager VirtualDesktopManager;
		internal static IApplicationViewCollection ApplicationViewCollection;
		internal static IVirtualDesktopPinnedApps VirtualDesktopPinnedApps;

		internal static IVirtualDesktop GetDesktop(int index)
		{   // get desktop with index
			int count = VirtualDesktopManagerInternal.GetCount();
			if (index < 0 || index >= count) throw new ArgumentOutOfRangeException("index");
			IObjectArray desktops;
			VirtualDesktopManagerInternal.GetDesktops(out desktops);
			object objdesktop;
			desktops.GetAt(index, typeof(IVirtualDesktop).GUID, out objdesktop);
			Marshal.ReleaseComObject(desktops);
			return (IVirtualDesktop)objdesktop;
		}

		internal static int GetDesktopIndex(IVirtualDesktop desktop)
		{ // get index of desktop
			int index = -1;
			Guid IdSearch = desktop.GetId();
			IObjectArray desktops;
			VirtualDesktopManagerInternal.GetDesktops(out desktops);
			object objdesktop;
			for (int i = 0; i < VirtualDesktopManagerInternal.GetCount(); i++)
			{
				desktops.GetAt(i, typeof(IVirtualDesktop).GUID, out objdesktop);
				if (IdSearch.CompareTo(((IVirtualDesktop)objdesktop).GetId()) == 0)
				{
					index = i;
					break;
				}
			}
			Marshal.ReleaseComObject(desktops);
			return index;
		}

		internal static IApplicationView GetApplicationView(this IntPtr hWnd)
		{ // get application view to window handle
			IApplicationView view;
			ApplicationViewCollection.GetViewForHwnd(hWnd, out view);
			return view;
		}

		internal static string GetAppId(IntPtr hWnd)
		{ // get Application ID to window handle
			string appId;
			hWnd.GetApplicationView().GetAppUserModelId(out appId);
			return appId;
		}
	}
	#endregion

	#region public interface
	public class WindowInformation
	{ // stores window informations
		public string Title { get; set; }
		public int Handle { get; set; }
	}

	public class Desktop
	{
		// get process id to window handle
		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		// get thread id of current process
		[DllImport("kernel32.dll")]
		static extern uint GetCurrentThreadId();

		// attach input to thread
		[DllImport("user32.dll")]
		static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		// get handle of active window
		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		// try to set foreground window
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)] static extern bool SetForegroundWindow(IntPtr hWnd);

		// send message to window
		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
		private const int SW_MINIMIZE = 6;

		private static readonly Guid AppOnAllDesktops = new Guid("BB64D5B7-4DE3-4AB2-A87C-DB7601AEA7DC");
		private static readonly Guid WindowOnAllDesktops = new Guid("C2DDEA68-66F2-4CF9-8264-1BFD00FBBBAC");

		private IVirtualDesktop ivd;
		private Desktop(IVirtualDesktop desktop) { this.ivd = desktop; }

		public override int GetHashCode()
		{ // get hash
			return ivd.GetHashCode();
		}

		public override bool Equals(object obj)
		{ // compare with object
			var desk = obj as Desktop;
			return desk != null && object.ReferenceEquals(this.ivd, desk.ivd);
		}

		public static int Count
		{ // return the number of desktops
			get { return DesktopManager.VirtualDesktopManagerInternal.GetCount(); }
		}

		public static Desktop Current
		{ // returns current desktop
			get { return new Desktop(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop()); }
		}

		public static Desktop FromIndex(int index)
		{ // return desktop object from index (-> index = 0..Count-1)
			return new Desktop(DesktopManager.GetDesktop(index));
		}

		public static Desktop FromWindow(IntPtr hWnd)
		{ // return desktop object to desktop on which window <hWnd> is displayed
			if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
			Guid id = DesktopManager.VirtualDesktopManager.GetWindowDesktopId(hWnd);
			if ((id.CompareTo(AppOnAllDesktops) == 0) || (id.CompareTo(WindowOnAllDesktops) == 0))
				return new Desktop(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop());
			else
				return new Desktop(DesktopManager.VirtualDesktopManagerInternal.FindDesktop(ref id));
		}

		public static int FromDesktop(Desktop desktop)
		{ // return index of desktop object or -1 if not found
			return DesktopManager.GetDesktopIndex(desktop.ivd);
		}

		public static string DesktopNameFromDesktop(Desktop desktop)
		{ // return name of desktop or "Desktop n" if it has no name

			// get desktop name
			string desktopName = null;
			try
			{
				desktopName = desktop.ivd.GetName();
			}
			catch { }

			// no name found, generate generic name
			if (string.IsNullOrEmpty(desktopName))
			{ // create name "Desktop n" (n = number starting with 1)
				desktopName = "Desktop " + (DesktopManager.GetDesktopIndex(desktop.ivd) + 1).ToString();
			}
			return desktopName;
		}

		public static string DesktopNameFromIndex(int index)
		{ // return name of desktop from index (-> index = 0..Count-1) or "Desktop n" if it has no name

			// get desktop name
			string desktopName = null;
			try
			{
				desktopName = DesktopManager.GetDesktop(index).GetName();
			}
			catch { }

			// no name found, generate generic name
			if (string.IsNullOrEmpty(desktopName))
			{ // create name "Desktop n" (n = number starting with 1)
				desktopName = "Desktop " + (index + 1).ToString();
			}
			return desktopName;
		}

		public static bool HasDesktopNameFromIndex(int index)
		{ // return true is desktop is named or false if it has no name

			// read desktop name in registry
			string desktopName = null;
			try
			{
				desktopName = DesktopManager.GetDesktop(index).GetName();
			}
			catch { }

			// name found?
			if (string.IsNullOrEmpty(desktopName))
				return false;
			else
				return true;
		}

		public static string DesktopWallpaperFromIndex(int index)
		{ // return name of desktop wallpaper from index (-> index = 0..Count-1)

			// get desktop name
			string desktopwppath = "";
			try
			{
				desktopwppath = DesktopManager.GetDesktop(index).GetWallpaperPath();
			}
			catch { }

			return desktopwppath;
		}

		public static int SearchDesktop(string partialName)
		{ // get index of desktop with partial name, return -1 if no desktop found
			int index = -1;

			for (int i = 0; i < DesktopManager.VirtualDesktopManagerInternal.GetCount(); i++)
			{ // loop through all virtual desktops and compare partial name to desktop name
				if (DesktopNameFromIndex(i).ToUpper().IndexOf(partialName.ToUpper()) >= 0)
				{
					index = i;
					break;
				}
			}

			return index;
		}

		public static Desktop Create()
		{ // create a new desktop
			return new Desktop(DesktopManager.VirtualDesktopManagerInternal.CreateDesktop());
		}

		public void Remove(Desktop fallback = null)
		{ // destroy desktop and switch to <fallback>
			IVirtualDesktop fallbackdesktop;
			if (fallback == null)
			{ // if no fallback is given use desktop to the left except for desktop 0.
				Desktop dtToCheck = new Desktop(DesktopManager.GetDesktop(0));
				if (this.Equals(dtToCheck))
				{ // desktop 0: set fallback to second desktop (= "right" desktop)
					DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 4, out fallbackdesktop); // 4 = RightDirection
				}
				else
				{ // set fallback to "left" desktop
					DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 3, out fallbackdesktop); // 3 = LeftDirection
				}
			}
			else
				// set fallback desktop
				fallbackdesktop = fallback.ivd;

			DesktopManager.VirtualDesktopManagerInternal.RemoveDesktop(ivd, fallbackdesktop);
		}

		public static void RemoveAll()
		{ // remove all desktops but visible
			int desktopcount = DesktopManager.VirtualDesktopManagerInternal.GetCount();
			int desktopcurrent = DesktopManager.GetDesktopIndex(DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop());

			if (desktopcurrent < desktopcount - 1)
			{ // remove all desktops "right" from current
				for (int i = desktopcount - 1; i > desktopcurrent; i--)
					DesktopManager.VirtualDesktopManagerInternal.RemoveDesktop(DesktopManager.GetDesktop(i), DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop());
			}
			if (desktopcurrent > 0)
			{ // remove all desktops "left" from current
				for (int i = 0; i < desktopcurrent; i++)
					DesktopManager.VirtualDesktopManagerInternal.RemoveDesktop(DesktopManager.GetDesktop(0), DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop());
			}
		}

		public void Move(int index)
		{ // move current desktop to desktop in index (-> index = 0..Count-1)
			DesktopManager.VirtualDesktopManagerInternal.MoveDesktop(ivd, index);
		}

		public void SetName(string Name)
		{ // set name for desktop, empty string removes name
			DesktopManager.VirtualDesktopManagerInternal.SetDesktopName(this.ivd, Name);
		}

		public void SetWallpaperPath(string Path)
		{ // set path for wallpaper, empty string removes path
			if (string.IsNullOrEmpty(Path)) throw new ArgumentNullException();
			DesktopManager.VirtualDesktopManagerInternal.SetDesktopWallpaper(this.ivd, Path);
		}

		public static void SetAllWallpaperPaths(string Path)
		{ // set wallpaper path for all desktops
			if (string.IsNullOrEmpty(Path)) throw new ArgumentNullException();
			DesktopManager.VirtualDesktopManagerInternal.UpdateWallpaperPathForAllDesktops(Path);
		}

		public bool IsVisible
		{ // return true if this desktop is the current displayed one
			get { return object.ReferenceEquals(ivd, DesktopManager.VirtualDesktopManagerInternal.GetCurrentDesktop()); }
		}

		public void MakeVisible()
		{ // make this desktop visible
			WindowInformation wi = FindWindow("Program Manager");

			// activate desktop to prevent flashing icons in taskbar
			int dummy;
			uint DesktopThreadId = GetWindowThreadProcessId(new IntPtr(wi.Handle), out dummy);
			uint ForegroundThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out dummy);
			uint CurrentThreadId = GetCurrentThreadId();

			if ((DesktopThreadId != 0) && (ForegroundThreadId != 0) && (ForegroundThreadId != CurrentThreadId))
			{
				AttachThreadInput(DesktopThreadId, CurrentThreadId, true);
				AttachThreadInput(ForegroundThreadId, CurrentThreadId, true);
				SetForegroundWindow(new IntPtr(wi.Handle));
				AttachThreadInput(ForegroundThreadId, CurrentThreadId, false);
				AttachThreadInput(DesktopThreadId, CurrentThreadId, false);
			}

			DesktopManager.VirtualDesktopManagerInternal.SwitchDesktop(ivd);

			// direct desktop to give away focus
			ShowWindow(new IntPtr(wi.Handle), SW_MINIMIZE);
		}

		public Desktop Left
		{ // return desktop at the left of this one, null if none
			get
			{
				IVirtualDesktop desktop;
				int hr = DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 3, out desktop); // 3 = LeftDirection
				if (hr == 0)
					return new Desktop(desktop);
				else
					return null;
			}
		}

		public Desktop Right
		{ // return desktop at the right of this one, null if none
			get
			{
				IVirtualDesktop desktop;
				int hr = DesktopManager.VirtualDesktopManagerInternal.GetAdjacentDesktop(ivd, 4, out desktop); // 4 = RightDirection
				if (hr == 0)
					return new Desktop(desktop);
				else
					return null;
			}
		}

		public void MoveWindow(IntPtr hWnd)
		{ // move window to this desktop
			int processId;
			if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
			GetWindowThreadProcessId(hWnd, out processId);

			if (System.Diagnostics.Process.GetCurrentProcess().Id == processId)
			{ // window of process
				try // the easy way (if we are owner)
				{
					DesktopManager.VirtualDesktopManager.MoveWindowToDesktop(hWnd, ivd.GetId());
				}
				catch // window of process, but we are not the owner
				{
					IApplicationView view;
					DesktopManager.ApplicationViewCollection.GetViewForHwnd(hWnd, out view);
					DesktopManager.VirtualDesktopManagerInternal.MoveViewToDesktop(view, ivd);
				}
			}
			else
			{ // window of other process
				IApplicationView view;
				DesktopManager.ApplicationViewCollection.GetViewForHwnd(hWnd, out view);
				try
				{
					DesktopManager.VirtualDesktopManagerInternal.MoveViewToDesktop(view, ivd);
				}
				catch
				{ // could not move active window, try main window (or whatever windows thinks is the main window)
					DesktopManager.ApplicationViewCollection.GetViewForHwnd(System.Diagnostics.Process.GetProcessById(processId).MainWindowHandle, out view);
					DesktopManager.VirtualDesktopManagerInternal.MoveViewToDesktop(view, ivd);
				}
			}
		}

		public void MoveActiveWindow()
		{ // move active window to this desktop
			MoveWindow(GetForegroundWindow());
		}

		public bool HasWindow(IntPtr hWnd)
		{ // return true if window is on this desktop
			if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
			Guid id = DesktopManager.VirtualDesktopManager.GetWindowDesktopId(hWnd);
			if ((id.CompareTo(AppOnAllDesktops) == 0) || (id.CompareTo(WindowOnAllDesktops) == 0))
				return true;
			else
				return ivd.GetId() == id;
		}

		public static bool IsWindowPinned(IntPtr hWnd)
		{ // return true if window is pinned to all desktops
			if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
			return DesktopManager.VirtualDesktopPinnedApps.IsViewPinned(hWnd.GetApplicationView());
		}

		public static void PinWindow(IntPtr hWnd)
		{ // pin window to all desktops
			if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
			var view = hWnd.GetApplicationView();
			if (!DesktopManager.VirtualDesktopPinnedApps.IsViewPinned(view))
			{ // pin only if not already pinned
				DesktopManager.VirtualDesktopPinnedApps.PinView(view);
			}
		}

		public static void UnpinWindow(IntPtr hWnd)
		{ // unpin window from all desktops
			if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
			var view = hWnd.GetApplicationView();
			if (DesktopManager.VirtualDesktopPinnedApps.IsViewPinned(view))
			{ // unpin only if not already unpinned
				DesktopManager.VirtualDesktopPinnedApps.UnpinView(view);
			}
		}

		public static bool IsApplicationPinned(IntPtr hWnd)
		{ // return true if application for window is pinned to all desktops
			if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
			return DesktopManager.VirtualDesktopPinnedApps.IsAppIdPinned(DesktopManager.GetAppId(hWnd));
		}

		public static void PinApplication(IntPtr hWnd)
		{ // pin application for window to all desktops
			if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
			string appId = DesktopManager.GetAppId(hWnd);
			if (!DesktopManager.VirtualDesktopPinnedApps.IsAppIdPinned(appId))
			{ // pin only if not already pinned
				DesktopManager.VirtualDesktopPinnedApps.PinAppID(appId);
			}
		}

		public static void UnpinApplication(IntPtr hWnd)
		{ // unpin application for window from all desktops
			if (hWnd == IntPtr.Zero) throw new ArgumentNullException();
			var view = hWnd.GetApplicationView();
			string appId = DesktopManager.GetAppId(hWnd);
			if (DesktopManager.VirtualDesktopPinnedApps.IsAppIdPinned(appId))
			{ // unpin only if pinned
				DesktopManager.VirtualDesktopPinnedApps.UnpinAppID(appId);
			}
		}

		// prepare callback function for window enumeration
		private delegate bool CallBackPtr(int hwnd, int lParam);
		private static CallBackPtr callBackPtr = Callback;
		// list of window informations
		private static List<WindowInformation> WindowInformationList = new List<WindowInformation>();

		// enumerate windows
		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumWindows(CallBackPtr lpEnumFunc, IntPtr lParam);

		// get window title length
		[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		// get window title
		[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		// callback function for window enumeration
		private static bool Callback(int hWnd, int lparam)
		{
			int length = GetWindowTextLength((IntPtr)hWnd);
			if (length > 0)
			{
				StringBuilder sb = new StringBuilder(length + 1);
				if (GetWindowText((IntPtr)hWnd, sb, sb.Capacity) > 0)
				{ WindowInformationList.Add(new WindowInformation { Handle = hWnd, Title = sb.ToString() }); }
			}
			return true;
		}

		// get list of all windows with title
		public static List<WindowInformation> GetWindows()
		{
			WindowInformationList = new List<WindowInformation>();
			EnumWindows(callBackPtr, IntPtr.Zero);
			return WindowInformationList;
		}

		// find first window with string in title
		public static WindowInformation FindWindow(string WindowTitle)
		{
			WindowInformationList = new List<WindowInformation>();
			EnumWindows(callBackPtr, IntPtr.Zero);
			WindowInformation result = WindowInformationList.Find(x => x.Title.IndexOf(WindowTitle, StringComparison.OrdinalIgnoreCase) >= 0);
			return result;
		}
	}
	#endregion
}

namespace VDeskTool
{

    public class MainForm : Form
    {

        private int hight = 3;
        private int width = 3;

        private KeyboardHook2 keyboardHook;
        private NotifyIcon notifyIcon;
        private int current = 0;
        public MainForm()
        {

            hight = VWinMover.Properties.Settings.Default.window_height;
            width = VWinMover.Properties.Settings.Default.window_width;

            this.ShowInTaskbar = false;
            this.setComponents();

            // キーボードフックの設定
            keyboardHook = new KeyboardHook2();

            keyboardHook.KeyDown += KeyboardHook_KeyDown;

        }

        // キーボードフックのイベントハンドラ
        private void KeyboardHook_KeyDown(object sender, KeyEventArgs e)
        {


            if (ModifierKeys == Keys.Control || ModifierKeys == (Keys.Control | Keys.Shift | Keys.Alt))
            {

                if (e.KeyCode == Keys.Right)
                {
                    int nextId = current + 1;
                    if ((current + 1) % width != 0)
                    {
                        if (ModifierKeys == (Keys.Control | Keys.Shift))
                        {
                            Task.Run(() =>
                                MoveActiveWindow(nextId)
                             );
                        }


                        Task.Run(() =>
                            DoAsyncWork(nextId)
                    );
                    }
                }
                else if (e.KeyCode == Keys.Left)
                {
                    int nextId = current - 1;
                    if ((current + 1) % width != 1)
                    {
                        if (ModifierKeys == (Keys.Control | Keys.Shift))
                        {
                            Task.Run(() =>
                                MoveActiveWindow(nextId)
                             );
                        }
                        Task.Run(() =>
                                 DoAsyncWork(nextId)

                            );
                    }
                }
                else if (e.KeyCode == Keys.Down)
                {
                    int nextId = current + width;
                    if (current + 1 <= width * hight - width)
                    {
                        if (ModifierKeys == (Keys.Control | Keys.Shift))
                        {
                            Task.Run(() =>
                                MoveActiveWindow(nextId)
                             );
                        }
                        Task.Run(() =>
                             DoAsyncWork(nextId)

                        );
                    }
                }
                else if (e.KeyCode == Keys.Up)
                {
                    int nextId = current - width;
                    if (current + 1 >= width + 1)
                    {
                        if (ModifierKeys == (Keys.Control | Keys.Shift))
                        {
                            Task.Run(() =>
                                MoveActiveWindow(nextId)
                             );
                        }
                        Task.Run(() =>
                             DoAsyncWork(nextId)

                        );
                    }
                }
                // アクティブウインドウの移動
                else if (e.KeyCode == Keys.D1 || e.KeyCode == Keys.D2 || e.KeyCode == Keys.D3
                    || e.KeyCode == Keys.D4 || e.KeyCode == Keys.D5 || e.KeyCode == Keys.D6 || e.KeyCode == Keys.D7 || e.KeyCode == Keys.D8
                    || e.KeyCode == Keys.D9)
                {
                    int keyValue = e.KeyCode.GetHashCode() - 48 - 1;
                    Task.Run(() =>
                         MoveActiveWindow(keyValue)
                    );
                }
            }

        }

        private async Task DoAsyncWork(int idx)
        {
            //MessageBox.Show(VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Current).ToString());
            // ここに非同期の処理を記述
            //await Task.Delay(100);
            VirtualDesktop.Desktop.FromIndex(idx).MakeVisible();

            // icon の修正
            if (idx == 0)
            {
                notifyIcon.Icon = VWinMover.Properties.Resources._1;
            }
            else if (idx == 1)
            {
                notifyIcon.Icon = VWinMover.Properties.Resources._2;
            }
            else if (idx == 2)
            {
                notifyIcon.Icon = VWinMover.Properties.Resources._3;
            }
            else if (idx == 3)
            {
                notifyIcon.Icon = VWinMover.Properties.Resources._4;
            }
            else if (idx == 4)
            {
                notifyIcon.Icon = VWinMover.Properties.Resources._5;
            }
            else if (idx == 5)
            {
                notifyIcon.Icon = VWinMover.Properties.Resources._6;
            }
            else if (idx == 6)
            {
                notifyIcon.Icon = VWinMover.Properties.Resources._7;
            }
            else if (idx == 7)
            {
                notifyIcon.Icon = VWinMover.Properties.Resources._8;
            }
            else if (idx == 8)
            {
                notifyIcon.Icon = VWinMover.Properties.Resources._9;
            }

            current = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Current);

        }

        private async Task MoveActiveWindow(int toDeskTopIndex)
        {
            VirtualDesktop.Desktop.FromIndex(toDeskTopIndex).MoveActiveWindow();
        }


        // フォームが閉じられるときの処理
        protected override void OnFormClosing(FormClosingEventArgs e)
        {

            // 現在のデスクトップを残して削除
            VirtualDesktop.Desktop.RemoveAll();

            base.OnFormClosing(e);
        }

        private void Close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void setComponents()
        {
            // 通常のフォームのロジックを実行
            current = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Current);



            //現在のデスクトップの数
            int currentDeskTopCount = DesktopManager.VirtualDesktopManagerInternal.GetCount();

            for (int i = currentDeskTopCount; i < hight * width; i++)
            {
                // 一つ作成
                VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Create());

            }

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = VWinMover.Properties.Resources._1;
            notifyIcon.Visible = true;
            notifyIcon.Text = "VWinMover";
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripMenuItem menuItem = new ToolStripMenuItem();
            menuItem.Text = "&終了";
            menuItem.Click += new EventHandler(Close_Click);
            menu.Items.Add(menuItem);
            notifyIcon.ContextMenuStrip = menu;

        }



    }

    public class Program
    {


        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            MainForm mainForm = new MainForm();
            mainForm.WindowState = FormWindowState.Minimized; // アプリを最小化
            Application.Run(mainForm);

        }

        static void Application_Exit(object sender, EventArgs e)
        {
            // 現在のデスクトップを残して削除
            VirtualDesktop.Desktop.RemoveAll();

        }




        /*** TODO : copy below */

        static int GetMainWindowHandle(string ProcessName)
        { // retrieve main window handle to process name
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(ProcessName);
            int wHwnd = 0;

            if (processes.Length > 0)
            { // process found, get window handle
                wHwnd = (int)processes[0].MainWindowHandle;
            }

            return wHwnd;
        }

        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        static uint WM_CLOSE = 0x10;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        const int MAXTITLE = 255;
        private static IntPtr foundHandle;
        private static string foundTitle;
        private static string searchTitle;

        private static bool EnumWindowsProc(IntPtr hWnd, int lParam)
        {
            StringBuilder windowText = new StringBuilder(MAXTITLE);
            int titleLength = GetWindowText(hWnd, windowText, windowText.Capacity + 1);
            windowText.Length = titleLength;
            string title = windowText.ToString();

            if (!string.IsNullOrEmpty(title) && IsWindowVisible(hWnd))
            {
                if (title.ToUpper().IndexOf(searchTitle.ToUpper()) >= 0)
                {
                    foundHandle = hWnd;
                    foundTitle = title;
                    return false;
                }
            }
            return true;
        }

        private static IntPtr GetWindowFromTitle(string searchFor)
        {
            searchTitle = searchFor;
            EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProc);

            foundHandle = IntPtr.Zero;
            foundTitle = "";
            EnumDesktopWindows(IntPtr.Zero, enumfunc, IntPtr.Zero);
            if (foundHandle == IntPtr.Zero)
            {
                // Get the last Win32 error code
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != 0)
                { // error
                    Console.WriteLine("EnumDesktopWindows failed with code {0}.", errorCode);
                }
            }
            return foundHandle;
        }

        private static int iListDesktop;

        private static bool EnumWindowsProcToList(IntPtr hWnd, int lParam)
        {
            try
            {
                int iDesktopIndex = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.FromWindow(hWnd));
                if (iDesktopIndex == iListDesktop) Console.WriteLine(hWnd.ToInt32());
            }
            catch { }

            return true;
        }

        private static void ListWindowsOnDesktop(int DesktopIndex)
        {
            iListDesktop = DesktopIndex;
            EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProcToList);

            EnumDesktopWindows(IntPtr.Zero, enumfunc, IntPtr.Zero);
        }

        private static int iCloseDesktop;

        private static bool EnumWindowsProcToClose(IntPtr hWnd, int lParam)
        {
            try
            {
                int iDesktopIndex = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.FromWindow(hWnd));
                if (iDesktopIndex == iCloseDesktop) SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
            catch { }

            return true;
        }

        private static void CloseWindowsOnDesktop(int DesktopIndex)
        {
            iCloseDesktop = DesktopIndex;
            EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProcToClose);

            EnumDesktopWindows(IntPtr.Zero, enumfunc, IntPtr.Zero);
        }

        private static int iMoveWindowsDesktop1;
        private static int iMoveWindowsDesktop2;

        private static bool EnumWindowsProcToMoveWindows(IntPtr hWnd, int lParam)
        {
            try
            {
                int iDesktopIndex = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.FromWindow(hWnd));
                if (iDesktopIndex == iMoveWindowsDesktop1) VirtualDesktop.Desktop.FromIndex(iMoveWindowsDesktop2).MoveWindow(hWnd);
            }
            catch { }

            return true;
        }

        private static void MoveWindowsToDesktop(int MoveWindowsIndex1, int MoveWindowsIndex2)
        {
            iMoveWindowsDesktop1 = MoveWindowsIndex1;
            iMoveWindowsDesktop2 = MoveWindowsIndex2;
            EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProcToMoveWindows);

            EnumDesktopWindows(IntPtr.Zero, enumfunc, IntPtr.Zero);
        }

        private static int iSwapDesktop1;
        private static int iSwapDesktop2;

        private static bool EnumWindowsProcToSwap(IntPtr hWnd, int lParam)
        {
            StringBuilder windowText = new StringBuilder(MAXTITLE);
            int titleLength = GetWindowText(hWnd, windowText, windowText.Capacity + 1);
            windowText.Length = titleLength;
            string title = windowText.ToString();

            if (!string.IsNullOrEmpty(title) && IsWindowVisible(hWnd))
            {
                try
                {
                    int iDesktopIndex = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.FromWindow(hWnd));
                    if (iDesktopIndex == iSwapDesktop1) VirtualDesktop.Desktop.FromIndex(iSwapDesktop2).MoveWindow(hWnd);
                    if (iDesktopIndex == iSwapDesktop2) VirtualDesktop.Desktop.FromIndex(iSwapDesktop1).MoveWindow(hWnd);
                }
                catch { }
            }

            return true;
        }

        private static void SwapDesktops(int SwapIndex1, int SwapIndex2)
        {
            iSwapDesktop1 = SwapIndex1;
            iSwapDesktop2 = SwapIndex2;
            EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProcToSwap);

            EnumDesktopWindows(IntPtr.Zero, enumfunc, IntPtr.Zero);

            string desktopname1 = "";
            if (VirtualDesktop.Desktop.HasDesktopNameFromIndex(iSwapDesktop1))
                desktopname1 = VirtualDesktop.Desktop.DesktopNameFromIndex(iSwapDesktop1);
            string desktopname2 = "";
            if (VirtualDesktop.Desktop.HasDesktopNameFromIndex(iSwapDesktop2))
                desktopname2 = VirtualDesktop.Desktop.DesktopNameFromIndex(iSwapDesktop2);

            VirtualDesktop.Desktop.FromIndex(iSwapDesktop1).SetName(desktopname2);
            VirtualDesktop.Desktop.FromIndex(iSwapDesktop2).SetName(desktopname1);
        }

        static void HelpScreen()
        {
            Console.WriteLine("VirtualDesktop.exe\t\t\t\tMarkus Scholtes, 2023, v1.15\n");

            Console.WriteLine("Command line tool to manage the virtual desktops of Windows 11.");
            Console.WriteLine("Parameters can be given as a sequence of commands. The result - most of the");
            Console.WriteLine("times the number of the processed desktop - can be used as input for the next");
            Console.WriteLine("parameter. The result of the last command is returned as error level.");
            Console.WriteLine("Virtual desktop numbers start with 0.\n");
            Console.WriteLine("Parameters (leading / can be omitted or - can be used instead):\n");
            Console.WriteLine("/Help /h /?      this help screen.");
            Console.WriteLine("/Verbose /Quiet  enable verbose (default) or quiet mode (short: /v and /q).");
            Console.WriteLine("/Break /Continue break (default) or continue on error (short: /b and /co).");
            Console.WriteLine("/List            list all virtual desktops (short: /li).");
            Console.WriteLine("/Count           get count of virtual desktops to pipeline (short: /c).");
            Console.WriteLine("/GetDesktop:<n|s> get number of virtual desktop <n> or desktop with text <s> in");
            Console.WriteLine("                   name to pipeline (short: /gd).");
            Console.WriteLine("/GetCurrentDesktop  get number of current desktop to pipeline (short: /gcd).");
            Console.WriteLine("/Name[:<s>]      set name of desktop with number in pipeline (short: /na).");
            Console.WriteLine("/Wallpaper[:<s>] set wallpaper path of desktop with number in pipeline (short:");
            Console.WriteLine("                   /wp).");
            Console.WriteLine("/AllWallpapers:<s> set wallpaper path of all desktops (short: /awp).");
            Console.WriteLine("/IsVisible[:<n|s>] is desktop number <n>, desktop with text <s> in name or with");
            Console.WriteLine("                   number in pipeline visible (short: /iv)? Returns 0 for");
            Console.WriteLine("                   visible and 1 for invisible.");
            Console.WriteLine("/Switch[:<n|s>]  switch to desktop with number <n>, desktop with text <s> in");
            Console.WriteLine("                   name or with number in pipeline (short: /s).");
            Console.WriteLine("/Left            switch to virtual desktop to the left of the active desktop");
            Console.WriteLine("                   (short: /l).");
            Console.WriteLine("/Right           switch to virtual desktop to the right of the active desktop");
            Console.WriteLine("                   (short: /ri).");
            Console.WriteLine("/Wrap /NoWrap    /Left or /Right switch over or generate an error when the edge");
            Console.WriteLine("                   is reached (default)(short /w and /nw).");
            Console.WriteLine("/New             create new desktop (short: /n). Number is stored in pipeline.");
            Console.WriteLine("/Remove[:<n|s>]  remove desktop number <n>, desktop with text <s> in name or");
            Console.WriteLine("                   desktop with number in pipeline (short: /r).");
            Console.WriteLine("/RemoveAll       remove all desktops but visible (short: /ra).");
            Console.WriteLine("/SwapDesktop:<n|s>  swap desktop in pipeline with desktop number <n> or desktop");
            Console.WriteLine("                   with text <s> in name (short: /sd).");
            Console.WriteLine("/MoveDesktop:<n|s>  move desktop in pipeline to desktop number <n> or desktop");
            Console.WriteLine("                   with text <s> in name (short: /md).");
            Console.WriteLine("/MoveWindowsToDesktop:<n|s>  move windows on desktop in pipeline to desktop");
            Console.WriteLine("                   number <n> or desktop with text <s> in name (short: /mwtd).");
            Console.WriteLine("/MoveWindow:<s|n>  move process with name <s> or id <n> to desktop with number");
            Console.WriteLine("                   in pipeline (short: /mw).");
            Console.WriteLine("/MoveWindowHandle:<s|n>  move window with text <s> in title or handle <n> to");
            Console.WriteLine("                   desktop with number in pipeline (short: /mwh).");
            Console.WriteLine("/MoveActiveWindow  move active window to desktop with number in pipeline");
            Console.WriteLine("                   (short: /maw).");
            Console.WriteLine("/GetDesktopFromWindow:<s|n>  get desktop number where process with name <s> or");
            Console.WriteLine("                   id <n> is displayed (short: /gdfw).");
            Console.WriteLine("/GetDesktopFromWindowHandle:<s|n>  get desktop number where window with text");
            Console.WriteLine("                   <s> in title or handle <n> is displayed (short: /gdfwh).");
            Console.WriteLine("/IsWindowOnDesktop:<s|n>  check if process with name <s> or id <n> is on");
            Console.WriteLine("                   desktop with number in pipeline (short: /iwod). Returns 0");
            Console.WriteLine("                   for yes, 1 for no.");
            Console.WriteLine("/IsWindowHandleOnDesktop:<s|n>  check if window with text <s> in title or");
            Console.WriteLine("                   handle <n> is on desktop with number in pipeline");
            Console.WriteLine("                   (short: /iwhod). Returns 0 for yes, 1 for no.");
            Console.WriteLine("/ListWindowsOnDesktop[:<n|s>]  list handles of windows on desktop number <n>,");
            Console.WriteLine("                   desktop with text <s> in name or desktop with number in");
            Console.WriteLine("                   pipeline (short: /lwod).");
            Console.WriteLine("/CloseWindowsOnDesktop[:<n|s>]  close windows on desktop number <n>, desktop");
            Console.WriteLine("                   with text <s> in name or desktop with number in pipeline");
            Console.WriteLine("                   (short: /cwod).");
            Console.WriteLine("/PinWindow:<s|n>  pin process with name <s> or id <n> to all desktops");
            Console.WriteLine("                   (short: /pw).");
            Console.WriteLine("/PinWindowHandle:<s|n>  pin window with text <s> in title or handle <n> to all");
            Console.WriteLine("                   desktops (short: /pwh).");
            Console.WriteLine("/UnPinWindow:<s|n>  unpin process with name <s> or id <n> from all desktops");
            Console.WriteLine("                   (short: /upw).");
            Console.WriteLine("/UnPinWindowHandle:<s|n>  unpin window with text <s> in title or handle <n>");
            Console.WriteLine("                   from all desktops (short: /upwh).");
            Console.WriteLine("/IsWindowPinned:<s|n>  check if process with name <s> or id <n> is pinned to");
            Console.WriteLine("                   all desktops (short: /iwp). Returns 0 for yes, 1 for no.");
            Console.WriteLine("/IsWindowHandlePinned:<s|n>  check if window with text <s> in title or handle");
            Console.WriteLine("                   <n> is pinned to all desktops (short: /iwhp). Returns 0 for");
            Console.WriteLine("                   yes, 1 for no.");
            Console.WriteLine("/PinApplication:<s|n>  pin application with name <s> or id <n> to all desktops");
            Console.WriteLine("                   (short: /pa).");
            Console.WriteLine("/UnPinApplication:<s|n>  unpin application with name <s> or id <n> from all");
            Console.WriteLine("                   desktops (short: /upa).");
            Console.WriteLine("/IsApplicationPinned:<s|n>  check if application with name <s> or id <n> is");
            Console.WriteLine("                   pinned to all desktops (short: /iap). Returns 0 for yes, 1");
            Console.WriteLine("                   for no.");
            Console.WriteLine("/Calc:<n>        add <n> to result, negative values are allowed (short: /ca).");
            Console.WriteLine("/WaitKey         wait for key press (short: /wk).");
            Console.WriteLine("/Sleep:<n>       wait for <n> milliseconds (short: /sl).\n");
            Console.WriteLine("Hint: Instead of a desktop name you can use LAST or *LAST* to select the last");
            Console.WriteLine("virtual desktop.");
            Console.WriteLine("Hint: Insert ^^ somewhere in window title parameters to prevent finding the own");
            Console.WriteLine("window. ^ is removed before searching window titles.\n");
            Console.WriteLine("Examples:");
            Console.WriteLine("Virtualdesktop.exe /LIST");
            Console.WriteLine("Virtualdesktop.exe \"-Switch:Desktop 2\"");
            Console.WriteLine("Virtualdesktop.exe -New -Switch -GetCurrentDesktop");
            Console.WriteLine("Virtualdesktop.exe Q N /MOVEACTIVEWINDOW /SWITCH");
            Console.WriteLine("Virtualdesktop.exe sleep:200 gd:1 mw:notepad s");
            Console.WriteLine("Virtualdesktop.exe /Count /continue /Remove /Remove /Count");
            Console.WriteLine("Virtualdesktop.exe /Count /Calc:-1 /Switch");
            Console.WriteLine("VirtualDesktop.exe -IsWindowPinned:cmd");
            Console.WriteLine("if ERRORLEVEL 1 VirtualDesktop.exe PinWindow:cmd");
            Console.WriteLine("Virtualdesktop.exe -GetDesktop:*last* \"-MoveWindowHandle:note^^pad\"");
        }

    }
}
