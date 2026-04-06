using tulo.CoreLib.Components.ResultPattern;
using tulo.eInvoiceCreatorZUGFeRD.DTOs;

namespace tulo.eInvoiceCreatorZUGFeRD.Stores.Invoices;
/// <summary>
/// In-memory store for invoice position data.
/// Acts as the single source of truth for all invoice positions within one application session.
/// No database or external persistence is involved – all data lives in memory until the application closes.
/// </summary>
public interface IInvoicePositionStore
{
    /// <summary>
    /// Suggests the next available position number based on the current items in the store.
    /// Useful for pre-filling the position number field when adding a new invoice position.
    /// </summary>
    /// <returns>The suggested next position number.</returns>
    int SuggestNextPositionNo();

    /// <summary>
    /// Adds a new invoice position to the store.
    /// </summary>
    /// <param name="dto">The invoice position data to add.</param>
    /// <param name="desiredPositionNo">
    /// Optional. If provided, the position will be inserted at this position number.
    /// If <c>null</c>, the store assigns the next available position number automatically.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the <see cref="Guid"/> of the newly added position,
    /// or a failure result if the operation could not be completed.
    /// </returns>
    Task<OperationResult<Guid>> AddAsync(InvoicePositionDetailsDTO dto, int? desiredPositionNo = null);

    /// <summary>
    /// Updates an existing invoice position identified by its <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice position to update.</param>
    /// <param name="dto">The updated invoice position data.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the <see cref="Guid"/> of the updated position,
    /// or a failure result if no position with the given <paramref name="id"/> was found.
    /// </returns>
    Task<OperationResult<Guid>> UpdateAsync(Guid id, InvoicePositionDetailsDTO dto);

    /// <summary>
    /// Removes an invoice position from the store by its <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice position to delete.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the <see cref="Guid"/> of the deleted position,
    /// or a failure result if no position with the given <paramref name="id"/> was found.
    /// </returns>
    Task<OperationResult<Guid>> DeleteAsync(Guid id);

    /// <summary>
    /// Retrieves all invoice positions currently held in the store,
    /// ordered by their position number.
    /// </summary>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the list of all
    /// <see cref="InvoicePositionDetailsDTO"/> objects,
    /// or a failure result if the retrieval could not be completed.
    /// </returns>
    Task<OperationResult<List<InvoicePositionDetailsDTO>>> GetAllAsync();

    /// <summary>
    /// Updates the position number of an existing invoice position and re-orders
    /// all other positions accordingly to maintain a consistent sequence.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice position to reorder.</param>
    /// <param name="newPositionNo">The new position number to assign.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the updated list of all
    /// <see cref="InvoicePositionDetailsDTO"/> objects after reordering,
    /// or a failure result if the operation could not be completed.
    /// </returns>
    Task<OperationResult<List<InvoicePositionDetailsDTO>>> SetPositionNoAsync(Guid id, int newPositionNo);

    Task<OperationResult<Guid>> AddSubPositionAsync(Guid parentId, InvoicePositionDetailsDTO dto);
    int SuggestNextSubPositionNo(Guid parentId);
}

