namespace PornhubApiWrapper.Pagination;

public static class AsyncPaginator
{
    public static async IAsyncEnumerable<T> EnumerateAsync<T>(
        Func<int, int, CancellationToken, Task<IReadOnlyList<T>>> pageLoader,
        int startPage,
        int perPage,
        int? maxItems = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = startPage;
        var yielded = 0;
        var cap = maxItems ?? int.MaxValue;

        while (yielded < cap)
        {
            var items = await pageLoader(page, perPage, cancellationToken).ConfigureAwait(false);
            if (items.Count == 0)
            {
                yield break;
            }

            foreach (var item in items)
            {
                if (yielded++ >= cap)
                {
                    yield break;
                }

                yield return item;
            }

            page++;
        }
    }
}
