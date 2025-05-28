namespace Services.Helpers
{
    public static class TransactionExtensions
    {
        public static async Task<ApiResult<T>> ExecuteTransactionAsync<T>(
            this IUnitOfWork unitOfWork,
            Func<Task<ApiResult<T>>> operation)
        {
            using var tx = await unitOfWork.BeginTransactionAsync();
            var result = await operation();
            if (result.IsSuccess)
                await tx.CommitAsync();
            else
                await tx.RollbackAsync();
            return result;
        }
    }
}