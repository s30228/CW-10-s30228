namespace CW10.DTOs;

public class PaginatedResult<TripGetDTO>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int AllPages { get; set; }
    public IEnumerable<TripGetDTO> Trips { get; set; } = Enumerable.Empty<TripGetDTO>();
}

public class TripGetDTO
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime DateFrom { get; set; }

    public DateTime DateTo { get; set; }

    public int MaxPeople { get; set; }
    
    public IEnumerable<CountryDTO> Countries { get; set; }

    public IEnumerable<ClientDTO> Clients { get; set; }
}

public class CountryDTO
{
    public string Name { get; set; } = null!;
}

public class ClientDTO
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;
}

