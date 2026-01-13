using ALS.Core.Contracts;

namespace ALS.Core.Extensions
{
    public static class AuditSearchRequestExtensions
    {
        public static AuditSearchRequest Normalize(this AuditSearchRequest r)
        {
            var page = r.Page < 1 ? 1 : r.Page;
            var pageSize = r.PageSize is < 1 ? 50 : (r.PageSize > 200 ? 200 : r.PageSize);

            return r with { Page = page, PageSize = pageSize };
        }
    }
}
