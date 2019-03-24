// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelayCommandExtensions.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the RelayCommandExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace OfficeRibbonXEditor.Extensions
{
    using GalaSoft.MvvmLight.Command;

    public static class RelayCommandExtensions
    {
        /// <summary>
        /// Executes the command passing null as the argument. This is especially useful for those commands which do
        /// not expect any argument (i.e. the majority of the ones in this project).
        /// </summary>
        /// <param name="command">The command to execute</param>
        public static void Execute(this RelayCommand command)
        {
            command.Execute(null);
        }
    }
}
