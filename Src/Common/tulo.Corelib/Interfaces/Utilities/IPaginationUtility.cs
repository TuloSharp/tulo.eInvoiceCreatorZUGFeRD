namespace tulo.CoreLib.Interfaces.Utilities
{
    public interface IPaginationUtility
    {
        /// <summary>
        /// stored the last current page
        /// </summary>
        int StoredCurrentPage { get; set; }

        /// <summary>
        /// event to schange the valeu in property StoredCurrentPage
        /// </summary>
        event Action StoredCurrentPageChanged;
    }
}