using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Services.Extensions
{
    public static class TransactionExtensions
    {
        /// <summary>
        /// Executes an operation within a database transaction
        /// </summary>
        /// <typeparam name="T">The type of result returned by the operation</typeparam>
        /// <param name="unitOfWork">The unit of work instance</param>
        /// <param name="operation">The operation to execute within the transaction</param>
        /// <param name="isolationLevel">The transaction isolation level</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the operation</returns>
        public static async Task<ApiResult<T>> ExecuteTransactionAsync<T>(
            this IUnitOfWork unitOfWork,
            Func<Task<ApiResult<T>>> operation,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            // Check if there's already an active transaction
            if (unitOfWork.HasActiveTransaction)
            {
                // If there's already a transaction, just execute the operation
                // This allows for nested transaction calls
                return await operation();
            }

            IDbContextTransaction? transaction = null;
            try
            {
                // Begin transaction
                transaction = await unitOfWork.BeginTransactionAsync(isolationLevel, cancellationToken);

                // Execute the operation
                var result = await operation();

                // Commit or rollback based on result
                if (result.IsSuccess)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
                else
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation
                if (transaction != null)
                {
                    await SafeRollbackAsync(transaction, CancellationToken.None);
                }
                throw;
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                if (transaction != null)
                {
                    await SafeRollbackAsync(transaction, cancellationToken);
                }

                // Return failure result instead of throwing
                return ApiResult<T>.Failure(new Exception($"Transaction failed: {ex.Message}"));
            }
            finally
            {
                // Clean up transaction
                transaction?.Dispose();
            }
        }

        /// <summary>
        /// Executes an operation within a database transaction (non-generic version for operations that don't return data)
        /// </summary>
        /// <param name="unitOfWork">The unit of work instance</param>
        /// <param name="operation">The operation to execute within the transaction</param>
        /// <param name="isolationLevel">The transaction isolation level</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the operation</returns>
        public static async Task<ApiResult<bool>> ExecuteTransactionAsync(
            this IUnitOfWork unitOfWork,
            Func<Task<ApiResult<bool>>> operation,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                return await operation();
            }

            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await unitOfWork.BeginTransactionAsync(isolationLevel, cancellationToken);

                var result = await operation();

                if (result.IsSuccess)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
                else
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                if (transaction != null)
                {
                    await SafeRollbackAsync(transaction, CancellationToken.None);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await SafeRollbackAsync(transaction, cancellationToken);
                }

                return ApiResult<bool>.Failure(new Exception($"Transaction failed: {ex.Message}"));
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        /// <summary>
        /// Executes an operation within a database transaction (for operations that return void/Task)
        /// </summary>
        /// <param name="unitOfWork">The unit of work instance</param>
        /// <param name="operation">The operation to execute within the transaction</param>
        /// <param name="isolationLevel">The transaction isolation level</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The result of the operation</returns>
        public static async Task<ApiResult<bool>> ExecuteTransactionAsync(
            this IUnitOfWork unitOfWork,
            Func<Task> operation,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            CancellationToken cancellationToken = default)
        {
            if (unitOfWork.HasActiveTransaction)
            {
                try
                {
                    await operation();
                    return ApiResult<bool>.Success(true, "Operation completed successfully");
                }
                catch (Exception ex)
                {
                    return ApiResult<bool>.Failure(ex);
                }
            }

            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await unitOfWork.BeginTransactionAsync(isolationLevel, cancellationToken);

                await operation();

                await transaction.CommitAsync(cancellationToken);
                return ApiResult<bool>.Success(true, "Transaction completed successfully");
            }
            catch (OperationCanceledException)
            {
                if (transaction != null)
                {
                    await SafeRollbackAsync(transaction, CancellationToken.None);
                }
                throw;
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await SafeRollbackAsync(transaction, cancellationToken);
                }

                return ApiResult<bool>.Failure(new Exception($"Transaction failed: {ex.Message}"));
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        /// <summary>
        /// Safely rollback a transaction, catching and logging any rollback exceptions
        /// </summary>
        /// <param name="transaction">The transaction to rollback</param>
        /// <param name="cancellationToken">Cancellation token</param>
        private static async Task SafeRollbackAsync(IDbContextTransaction transaction, CancellationToken cancellationToken)
        {
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                // Log the rollback exception if you have a logger
                // _logger?.LogError(rollbackEx, "Failed to rollback transaction");

                // Don't throw rollback exceptions to avoid masking the original exception
            }
        }
    }
}