using GSoft.Cqrs.Registrations;

namespace GSoft.Cqrs;

internal static class ValidationUtils
{
    public static bool TryGetDuplicateRegistrations(
        IEnumerable<QueryRegistration> queryRegistrations,
        IEnumerable<StreamQueryRegistration> asyncQueryRegistrations,
        IEnumerable<CommandRegistration> commandRegistrations,
        out IEnumerable<string>? duplicates)
    {
        var duplicateQueries = queryRegistrations.GroupBy(x => x.QueryType)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key.Name)
            .ToList();

        var duplicateStreams = asyncQueryRegistrations.GroupBy(x => x.QueryType)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key.Name)
            .ToList();

        var duplicateCommand = commandRegistrations.GroupBy(x => x.CommandType)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key.Name)
            .ToList();

        if (duplicateQueries.Any() || duplicateStreams.Any() || duplicateCommand.Any())
        {
            duplicates = duplicateQueries.Concat(duplicateStreams).Concat(duplicateCommand);
            return true;
        }

        duplicates = null;
        return false;
    }
}