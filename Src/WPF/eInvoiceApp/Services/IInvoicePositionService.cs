using tulo.CoreLib.Components.ResultPattern;
using tulo.eInvoice.eInvoiceApp.DTOs;

namespace tulo.eInvoice.eInvoiceApp.Services;

/// <summary>
/// Provides CRUD operations and state tracking for invoice positions.
/// Raises events to notify subscribers when positions are loaded, created, updated, or deleted.
/// </summary>
public interface IInvoicePositionService
{
    /// <summary>
    /// Gets a value indicating whether the invoice positions have been successfully loaded.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Gets a value indicating whether the last create operation completed successfully.
    /// </summary>
    bool IsCreated { get; }

    /// <summary>
    /// Gets a value indicating whether the last update operation completed successfully.
    /// </summary>
    bool IsUpdated { get; }

    /// <summary>
    /// Gets a value indicating whether the last delete operation completed successfully.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Gets a value indicating whether all required fields for the current
    /// invoice position are filled and valid.
    /// </summary>
    bool AreRequiredFieldsFilled { get; }

    /// <summary>
    /// Gets the status message produced by the last service operation.
    /// Can be used to display feedback to the user (e.g. success or error details).
    /// </summary>
    string StatusMessage { get; }

    /// <summary>
    /// Gets the total number of invoice positions currently managed by the service.
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    /// Raised when the full list of invoice positions has been loaded.
    /// The payload contains all loaded <see cref="InvoicePositionDetailsDTO"/> entries.
    /// </summary>
    event Action<List<InvoicePositionDetailsDTO>>? InvoicePositionsLoaded;

    /// <summary>
    /// Raised when a new invoice position has been successfully created.
    /// The payload contains the newly created <see cref="InvoicePositionDetailsDTO"/>.
    /// </summary>
    event Action<InvoicePositionDetailsDTO>? InvoicePositionCreated;

    /// <summary>
    /// Raised when an existing invoice position has been successfully updated.
    /// The payload contains the updated <see cref="InvoicePositionDetailsDTO"/>.
    /// </summary>
    event Action<InvoicePositionDetailsDTO>? InvoicePositionUpdated;

    /// <summary>
    /// Raised when an invoice position has been successfully deleted.
    /// The payload contains the <see cref="Guid"/> of the deleted position.
    /// </summary>
    event Action<Guid>? InvoicePositionDeleted;

    /// <summary>
    /// Suggests the next available position number for a new invoice position.
    /// The suggested number is based on the current list of existing positions.
    /// </summary>
    /// <returns>The next recommended position number as an <see cref="int"/>.</returns>
    int SuggestNextPositionNo();

    /// <summary>
    /// Asynchronously loads all invoice positions and raises <see cref="InvoicePositionsLoaded"/>
    /// upon success.
    /// </summary>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the full list of
    /// <see cref="InvoicePositionDetailsDTO"/> entries, or an error result on failure.
    /// </returns>
    Task<OperationResult<List<InvoicePositionDetailsDTO>>> LoadAllInvoicePositionsAsync();

    /// <summary>
    /// Asynchronously creates a new invoice position and raises <see cref="InvoicePositionCreated"/>
    /// upon success.
    /// </summary>
    /// <param name="invPos">The invoice position data to create.</param>
    /// <param name="desiredPositionNo">
    /// An optional position number to assign. If <see langword="null"/>,
    /// the next available number is used.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the <see cref="Guid"/>
    /// of the newly created position, or an error result on failure.
    /// </returns>
    Task<OperationResult<Guid>> AddInvoicePositionAsync(InvoicePositionDetailsDTO invPos, int? desiredPositionNo = null);

    /// <summary>
    /// Asynchronously updates an existing invoice position and raises
    /// <see cref="InvoicePositionUpdated"/> upon success.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice position to update.</param>
    /// <param name="invPos">The updated invoice position data.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the <see cref="Guid"/>
    /// of the updated position, or an error result on failure.
    /// </returns>
    Task<OperationResult<Guid>> UpdateInvoicePositionAsync(Guid id, InvoicePositionDetailsDTO invPos);

    /// <summary>
    /// Asynchronously deletes the invoice position with the specified identifier
    /// and raises <see cref="InvoicePositionDeleted"/> upon success.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice position to delete.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the <see cref="Guid"/>
    /// of the deleted position, or an error result on failure.
    /// </returns>
    Task<OperationResult<Guid>> DeleteInvoicePositionAsync(Guid id);

    /// <summary>
    /// Asynchronously reassigns the position number of an existing invoice position.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice position to reorder.</param>
    /// <param name="newPositionNo">The new position number to assign.</param>
    /// <returns>
    /// An <see cref="OperationResult{T}"/> containing the updated list of
    /// <see cref="InvoicePositionDetailsDTO"/> entries reflecting the new order,
    /// or an error result on failure.
    /// </returns>
    Task<OperationResult<List<InvoicePositionDetailsDTO>>> SetInvoicePositionNoAsync(Guid id, int newPositionNo);

    /// <summary>
    /// Adds a new DETAIL sub-position under an existing GROUP position.
    /// </summary>
    /// <param name="parentId">
    /// The <see cref="Guid"/> of the parent GROUP position.
    /// Must refer to a position with <c>LineStatusReasonCode = "GROUP"</c>,
    /// otherwise the operation will fail.
    /// </param>
    /// <param name="subPos">
    /// The DTO containing the sub-position data to persist.
    /// <c>ParentPositionId</c> and <c>LineStatusReasonCode</c> will be enforced
    /// by the store regardless of what is passed here.
    /// </param>
    /// <returns>
    /// An <see cref="OperationResult{Guid}"/> containing the newly created sub-position Id on success,
    /// or an error message if the parent was not found, is not a GROUP, or the insert failed.
    /// </returns>
    Task<OperationResult<Guid>> AddSubInvoicePositionAsync(Guid parentId, InvoicePositionDetailsDTO subPos);
}

