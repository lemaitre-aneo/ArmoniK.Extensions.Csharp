/* SessionContext.cs is part of the Armonik SDK solution.

   Copyright (c) 2021-2021 ANEO.
     D. DUBUC (https://github.com/ddubuc)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

*/

using ArmoniK.DevelopmentKit.Common;

namespace ArmoniK.DevelopmentKit.SymphonyApi
{
  /// <summary>
  ///   Container for the information associated with a particular Session.
  ///   Such information may be required during the servicing of a task from a Session.
  /// </summary>
  [MarkDownDoc]
  public class SessionContext
  {
    /// <summary>
    /// </summary>
    public int TimeRemoteDebug;

    /// <summary>
    /// </summary>
    public bool IsDebugMode => TimeRemoteDebug > 0;

    /// <summary>
    /// </summary>
    /// <value></value>
    public string SessionId { get; set; }

    /// <summary>
    /// </summary>
    /// <value></value>
    public string ClientLibVersion { get; set; }
  }
}
