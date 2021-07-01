using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modding
{
    /// <summary>
    ///		
    /// </summary>
    public class InGameConsoleSettings
    {
        /// <summary>
        ///	    Wheter to use colors in the log console.
        /// </summary>
        public bool UseLogColors;

        /// <summary>
        ///     The color to use for Fine logging when UseLogColors is enabled
        /// </summary>
        public string FineColor = "grey";

        /// <summary>
        ///	    The color to use for Info logging when UseLogColors is enabled
        /// </summary>
        public string InfoColor = "cyan";

        /// <summary>
        ///	    The color to use for Debug logging when UseLogColors is enabled
        /// </summary>
        public string DebugColor = "white";

        /// <summary>
        ///	    The color to use for Warning logging when UseLogColors is enabled
        /// </summary>
        public string WarningColor = "yellow";

        /// <summary>
        ///	    The color to use for Error logging when UseLogColors is enabled
        /// </summary>
        public string ErrorColor = "red";

        /// <summary>
        ///	    The color to use when UseLogColors is disabled
        /// </summary>
        public string DefaultColor = "white";
    }
}
