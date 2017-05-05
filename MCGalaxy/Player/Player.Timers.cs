﻿/*
Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
Dual-licensed under the Educational Community License, Version 2.0 and
the GNU General Public License, Version 3 (the "Licenses"); you may
not use this file except in compliance with the Licenses. You may
obtain a copy of the Licenses at
http://www.opensource.org/licenses/ecl2.php
http://www.gnu.org/licenses/gpl-3.0.html
Unless required by applicable law or agreed to in writing,
software distributed under the Licenses are distributed on an "AS IS"
BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
or implied. See the Licenses for the specific language governing
permissions and limitations under the Licenses.
 */
using System;
using System.Data;
using System.IO;
using System.Timers;
using MCGalaxy.Commands.Chatting;
using MCGalaxy.SQL;
using MCGalaxy.Network;

namespace MCGalaxy {
    public sealed partial class Player : IDisposable {

        void InitTimers() {      
            checkTimer.Elapsed += CheckTimerElapsed;
            checkTimer.Start();
        }
        
        void CheckTimerElapsed(object sender, ElapsedEventArgs e) {
            if (name == "") return;
            SendRaw(Opcode.Ping);
            if (Server.afkminutes <= 0) return;
            if (DateTime.UtcNow < AFKCooldown) return;
            
            if (IsAfk) {
                int time = Server.afkkick;
                if (AutoAfk) time += Server.afkminutes;
                
                if (Server.afkkick > 0 && group.Permission < Server.afkkickperm) {
                    if (LastAction.AddMinutes(time) < DateTime.UtcNow)
                        Leave("Auto-kick, AFK for " + Server.afkkick + " minutes");
                }
                if (Moved()) CmdAfk.ToggleAfk(this, "");
            } else {
                if (Moved()) LastAction = DateTime.UtcNow;

                DateTime lastAction = LastAction;
                if (LastAction.AddMinutes(Server.afkminutes) < DateTime.UtcNow
                    && !String.IsNullOrEmpty(name)) {
                    CmdAfk.ToggleAfk(this, "auto: Not moved for " + Server.afkminutes + " minutes");
                    AutoAfk = true;
                    LastAction = lastAction;
                }
            }
        }
        
        bool Moved() { return lastRot.RotY != Rot.RotY || lastRot.HeadX != Rot.HeadX; }
        
        void DisposeTimer(Timer timer, ElapsedEventHandler handler) {
            // Note: Some frameworks throw an ObjectDisposedException, 
            //       if a timer has already been disposed and we try to stop it
            try {
                timer.Stop();
                timer.Elapsed -= handler;
                timer.Dispose();
            } catch (Exception ex) {
                Server.ErrorLog(ex);
            }
        }
    }
}
