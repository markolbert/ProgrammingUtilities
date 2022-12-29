// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of EFCoreUtilities.
//
// EFCoreUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// EFCoreUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with EFCoreUtilities. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.EntityFrameworkCore;

// ReSharper disable ExplicitCallerInfoArgument

namespace J4JSoftware.EFCoreUtilities;

public class ImportDataLayer : IImportDataLayer
{
    private readonly DbContext _dbContext;
    private readonly IJ4JLogger _logger;

    public ImportDataLayer(
        DbContext dbContext,
        IJ4JLogger logger
    )
    {
        _dbContext = dbContext;

        _logger = logger;
        _logger.SetLoggedType( GetType() );
    }

    public void LogPendingChanges() =>
        _logger.Information<string>( "{0}", _dbContext.ChangeTracker.DebugView.LongView );

    public bool SaveChanges()
    {
        try
        {
            _dbContext.SaveChanges();
            _dbContext.ChangeTracker.Clear();

            return true;
        }
        catch( DbUpdateException e )
        {
            _logger.Fatal( e.FormatDbException() );
            return false;
        }
        catch( Exception e )
        {
            _logger.Fatal( e.FormatException( "Exception thrown while saving database changes" ) );
            return false;
        }
    }

    public TEntity? GetPendingEntity<TEntity>(
        Func<TEntity, bool> matchFunc
    )
        where TEntity : class
    {
        return _dbContext.ChangeTracker
                         .Entries<TEntity>()
                         .FirstOrDefault( x => x.State == EntityState.Added
                                           && matchFunc( x.Entity ) )
                        ?.Entity;
    }

    public List<TEntity> GetPendingEntities<TEntity>(
        Func<TEntity, bool> matchFunc
    )
        where TEntity : class
    {
        return _dbContext.ChangeTracker
                         .Entries<TEntity>()
                         .Where( x => x.State == EntityState.Added
                                  && matchFunc( x.Entity ) )
                         .Select( x => x.Entity )
                         .ToList();
    }
}