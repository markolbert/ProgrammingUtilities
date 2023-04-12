#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// FileLocking.cs
//
// This file is part of JumpForJoy Software's DependencyInjection.
// 
// DependencyInjection is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// DependencyInjection is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with DependencyInjection. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Collections.Generic;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBePrivate.Local

namespace J4JSoftware.DependencyInjection;

// thanx to Waldemar Gałęzinowski for this
// modified to comply with Resharper feedback and my naming protocols
// https://stackoverflow.com/questions/317071/how-do-i-find-out-which-process-is-locking-a-file-using-net
public static class FileLocking
{
    [StructLayout(LayoutKind.Sequential)]
    private struct RmUniqueProcess
    {
        public int ProcessId;
        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    private const int RmRebootReasonNone = 0;
    private const int CchRmMaxAppName = 255;
    private const int CchRmMaxSvcName = 63;

    private enum RmAppType
    {
        RmUnknownApp = 0,
        RmMainWindow = 1,
        RmOtherWindow = 2,
        RmService = 3,
        RmExplorer = 4,
        RmConsole = 5,
        RmCritical = 1000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RmProcessInfo
    {
        public RmUniqueProcess Process;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchRmMaxAppName + 1)]
        public string AppName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchRmMaxSvcName + 1)]
        public string ServiceShortName;

        internal RmAppType ApplicationType;
        public uint AppStatus;
        public uint TSSessionId;
        [MarshalAs(UnmanagedType.Bool)]
        public bool Restartable;
    }

    // ReSharper disable once StringLiteralTypo
    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmRegisterResources(uint pSessionHandle,
                                          uint nFiles,
                                          string[] rgsFilenames,
                                          uint nApplications,
                                          [In] RmUniqueProcess[]? rgApplications,
                                          uint nServices,
                                          string[]? rgsServiceNames);

    // ReSharper disable once StringLiteralTypo
    [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
    private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

    // ReSharper disable once StringLiteralTypo
    [DllImport("rstrtmgr.dll")]
    private static extern int RmEndSession(uint pSessionHandle);

    // ReSharper disable once StringLiteralTypo
    [DllImport("rstrtmgr.dll")]
    private static extern int RmGetList(uint dwSessionHandle,
                                out uint pnProcInfoNeeded,
                                ref uint pnProcInfo,
                                [In, Out] RmProcessInfo[]? rgAffectedApps,
                                ref uint lpdwRebootReasons);

    /// <summary>
    /// Find out what process(es) have a lock on the specified file.
    /// </summary>
    /// <param name="path">Path of the file.</param>
    /// <returns>Processes locking the file</returns>
    /// <remarks>See also:
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/aa373661(v=vs.85).aspx
    /// http://wyupdate.googlecode.com/svn-history/r401/trunk/frmFilesInUse.cs (no copyright in code at time of viewing)
    /// 
    /// </remarks>
    public static List<Process> WhoIsLocking(string path)
    {
        var key = Guid.NewGuid().ToString();
        var processes = new List<Process>();

        var res = RmStartSession(out var handle, 0, key);
        if (res != 0) 
            throw new Exception("Could not begin restart session.  Unable to determine file locker.");

        try
        {
            const int errorMoreData = 234;
            uint pnProcInfo = 0,
                 rebootReasons = RmRebootReasonNone;

            var resources = new[] { path }; // Just checking on one resource.

            res = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

            if (res != 0) 
                throw new Exception("Could not register resource.");

            //Note: there's a race condition here -- the first call to RmGetList() returns
            //      the total number of process. However, when we call RmGetList() again to get
            //      the actual processes this number may have increased.
            res = RmGetList(handle, out var pnProcInfoNeeded, ref pnProcInfo, null, ref rebootReasons);

            if (res == errorMoreData)
            {
                // Create an array to store the process results
                var processInfo = new RmProcessInfo[pnProcInfoNeeded];
                pnProcInfo = pnProcInfoNeeded;

                // Get the list
                res = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref rebootReasons);
                if (res == 0)
                {
                    processes = new List<Process>((int)pnProcInfo);

                    // Enumerate all of the results and add them to the 
                    // list to be returned
                    for (var i = 0; i < pnProcInfo; i++)
                    {
                        try
                        {
                            processes.Add(Process.GetProcessById(processInfo[i].Process.ProcessId));
                        }
                        // catch the error -- in case the process is no longer running
                        catch (ArgumentException) { }
                    }
                }
                else throw new Exception("Could not list processes locking resource.");
            }
            else if (res != 0) throw new Exception("Could not list processes locking resource. Failed to get size of result.");
        }
        finally
        {
#pragma warning disable IDE0059
            // ReSharper disable once UnusedVariable
            var junk = RmEndSession(handle);
#pragma warning restore IDE0059
        }

        return processes;
    }
}