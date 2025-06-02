using CW10.Data;
using CW10.DTOs;
using CW10.Exceptions;
using CW10.Models;
using Microsoft.EntityFrameworkCore;

namespace CW10.Services;

public class DbService(MasterContext data) : IDbService
{
    public async Task<PaginatedResult<TripGetDTO>> GetTripsInfoAsunc(int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        var allItems = await data.Trips.CountAsync();
        var allPages = (int)Math.Ceiling(allItems / (double)pageSize);
        
        var trips = await data.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TripGetDTO
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries
                    .Select(c => new CountryDTO
                    {
                        Name = c.Name
                    }),
                Clients = t.ClientTrips
                    .Select(ct => new ClientDTO
                    {
                        FirstName = ct.IdClientNavigation.FirstName,
                        LastName = ct.IdClientNavigation.LastName
                    })
            })
            .ToListAsync();

        return new PaginatedResult<TripGetDTO>
        {
            Page = page,
            PageSize = pageSize,
            AllPages = allPages,
            Trips = trips
        };
    }

    public async Task DeleteClientAsync(int idClient)
    {
        var client = await data.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == idClient);

        if (client == null)
            throw new NotFoundException($"Client with id {idClient} not found");

        if (client.ClientTrips.Any())
            throw new BadRequestException(
                "Client cannot be deleted because they are assigned to one or more trips.");

        data.Clients.Remove(client);
        await data.SaveChangesAsync();
    }

    public async Task<ClientTripGetDTO> AssignClientToTripAsync(int idTrip, ClientCreateDTO dto)
    {
        var trip = await data.Trips
            .Include(t => t.ClientTrips)
            .FirstOrDefaultAsync(t => t.IdTrip == idTrip);
        
        // trip doesn't exist or ended
        if (trip == null || trip.DateFrom <= DateTime.Now)
            throw new NotFoundException($"Trip with id {idTrip} not found or has already started.");
        
        var clientCheck = await data.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.Pesel == dto.Pesel);

        if (clientCheck != null)
        {
            throw new BadRequestException($"Client with pesel {dto.Pesel} already exists.");
        }
        
        // nigdy tego nie sprawdzimy bo i tak rzucamy błąd
        // jeżeli klient istnieje, ale robię jak w poleceniu
        if (clientCheck != null)
        {
            bool alreadyAssigned = clientCheck.ClientTrips.Any(ct => ct.IdTrip == idTrip);
            if (alreadyAssigned)
                throw new BadRequestException("Client with given PESEL is already assigned to this trip.");
        }
        
        var transaction = await data.Database.BeginTransactionAsync();
        try
        {
            var client = new Client
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Telephone = dto.Telephone,
                Pesel = dto.Pesel
            };
            
            data.Clients.Add(client);
            await data.SaveChangesAsync();

            var clientTrip = new ClientTrip
            {
                IdClient = client.IdClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = dto.PaymentDate
            };

            data.ClientTrips.Add(clientTrip);
            await data.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ClientTripGetDTO
            {
                IdClient = client.IdClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = dto.PaymentDate
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}