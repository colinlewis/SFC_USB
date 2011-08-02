using System;
using System.Windows.Forms;

namespace SFC_USB
{
    ///<summary>
    /// Runs the application.
    ///</summary> 
    ///
    public class SFC_USB  
    {         
        internal static frmMain frmMy; 
        
        /// <summary>
        ///  Displays the application's main form.
        /// </summary> 
        public static void Main() 
        {
            frmMy = new frmMain();
            Application.Run(frmMy); 
        }  
    } 
} 
