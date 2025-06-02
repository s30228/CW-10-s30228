using CW10.DTOs;
using CW10.Models;

namespace CW10.Services;

public interface IDbService
{
    public Task<PaginatedResult<TripGetDTO>> GetTripsInfoAsunc(int page, int pageSize);
    public Task DeleteClientAsync(int idClient);
    public Task<ClientTripGetDTO> AssignClientToTripAsync(int idTrip, ClientCreateDTO dto);
}