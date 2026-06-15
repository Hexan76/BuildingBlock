using Framework.BuildingBlock.Domain.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Data;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;

namespace Framework.BuildingBlock;

[ExposeServices(typeof(IHashtExceptionHandler))]
public class DBExceptionHandler : IHashtExceptionHandler, ITransientDependency
{
    private readonly IStringLocalizer<BuildingBlockResource> L;

    public DBExceptionHandler(IStringLocalizer<BuildingBlockResource> localizer)
    {
        L = localizer;
    }

    public bool CanHandle(Exception ex)
    {
        return ex is AbpDbConcurrencyException
               || ex is DbUpdateConcurrencyException
               || ex is DbUpdateException
               || ex is DBConcurrencyException
               || ex is EntityNotFoundException
               || ex is SqlException;
    }

    public FrameworkRemoteErrorInfoDto Handle(
        Exception ex,
        bool sendExceptionsDetailsToClients,
        bool sendStackTraceToClients)
    {
        if (ex is EntityNotFoundException exception)
        {
            return CreateError(
                L["Errors.EntityNotFound"],
                ex,
                sendExceptionsDetailsToClients);

        }
        if (IsConcurrencyException(ex))
        {
            return CreateError(
                L["Errors.Concurrency"],
                ex,
                sendExceptionsDetailsToClients);
        }

        var sqlException = ExtractSqlException(ex);
        if (sqlException != null)
        {
            return HandleSqlException(sqlException, ex, sendExceptionsDetailsToClients);
        }

        if (ex is DbUpdateException)
        {
            return CreateError(
                L["Errors.DbUpdate"],
                ex,
                sendExceptionsDetailsToClients);
        }

        return CreateError(
            L["Errors.Unexpected"],
            ex,
            sendExceptionsDetailsToClients);
    }

    // ----------------- Private Methods -----------------

    private bool IsConcurrencyException(Exception ex)
    {
        return ex is AbpDbConcurrencyException
               || ex is DbUpdateConcurrencyException
               || ex is DBConcurrencyException;
    }

    private FrameworkRemoteErrorInfoDto HandleSqlException(
        SqlException sqlException,
        Exception originalException,
        bool sendDetails)
    {
        switch (sqlException.Number)
        {
            case 547:   // Foreign Key violation
                return CreateError(
                    L["Errors.ForeignKey"],
                    originalException,
                    sendDetails);

            case 2601:  // Unique index
            case 2627:  // Unique constraint
                return CreateError(
                    L["Errors.Duplicate"],
                    originalException,
                    sendDetails);

            case 1205:  // Deadlock
                return CreateError(
                    L["Errors.Deadlock"],
                    originalException,
                    sendDetails);

            default:
                return CreateError(
                    L["Errors.SqlGeneric"],
                    originalException,
                    sendDetails);
        }
    }

    private static SqlException? ExtractSqlException(Exception ex)
    {
        while (ex != null)
        {
            if (ex is SqlException sqlEx)
                return sqlEx;

            ex = ex.InnerException;
        }

        return null;
    }

    private FrameworkRemoteErrorInfoDto CreateError(
        string message,
        Exception ex,
        bool sendDetails)
    {
        return new FrameworkRemoteErrorInfoDto(message, null)
        {
            Details = sendDetails ? ex.ToString() : null
        };
    }
}
