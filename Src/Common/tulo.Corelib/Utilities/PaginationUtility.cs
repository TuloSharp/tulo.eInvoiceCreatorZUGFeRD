using tulo.CoreLib.Interfaces.Utilities;

namespace tulo.CoreLib.Utilities
{
    public class PaginationUtility : IPaginationUtility
    {
        private int _storedCurrentPage;
        public int StoredCurrentPage
        {
            get => _storedCurrentPage;
            set
            {
                if (_storedCurrentPage != value)
                {
                    _storedCurrentPage = value;
                    StoredCurrentPageChanged?.Invoke();
                }
            }
        }
        public event Action StoredCurrentPageChanged = delegate { };
    }
}
