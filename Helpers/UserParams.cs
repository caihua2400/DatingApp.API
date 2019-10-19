namespace DatingApp.API.Helpers
{
    public class UserParams
    {
        private const int MaxPageSize = 50; 
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10;

        public int PageSize {
            get
            {
                return this._pageSize;
            }
            set { this._pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }
    }
}