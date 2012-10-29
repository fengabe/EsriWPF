﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ORMapping;
using SOPFIM.Domain;

namespace SOPFIM.DataLayer
{
    public class DataService : IDataService
    {
        private readonly FileGDBWorkspaceFactory _workspaceFactory;
        private readonly IFeatureWorkspace _featureWorkspace;

        public DataService(string fileGeodatabase)
        {
            try
            {
            _workspaceFactory = new FileGDBWorkspaceFactory();
            _featureWorkspace = (IFeatureWorkspace)_workspaceFactory.OpenFromFile(fileGeodatabase, 0);

            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147467259)
                    throw new FileNotFoundException(string.Format("File geodatabase {0} was not found", fileGeodatabase), fileGeodatabase);
            }

        }

        public ITable GetTable(string tableName)
        {
            return _featureWorkspace.OpenTable(tableName);
        }

        public List<T> GeneralQuery<T>(ITable table, string whereClause) where T : EditableEntity, new()
        {
            IQueryFilter filter = string.IsNullOrEmpty(whereClause) ? null : new QueryFilter {WhereClause = whereClause};
            var result = new List<T>(table.Map<T>(filter));
            result.ForEach(x => x.IsDirty = false);
            return result;
        }


        public List<DomainRecord> GetDomain(string domainName)
        {
            return GetDomain(domainName, true);
        }

        public List<DomainRecord> GetDomain(string domainName, bool addEmptyRecord)
        {
            var w = (IWorkspaceDomains)_featureWorkspace;
            var d = (ICodedValueDomain)w.DomainByName[domainName];
            var result = new List<DomainRecord>();
            if (addEmptyRecord)
                result.Add(new DomainRecord() { Code = null, Description = string.Empty } );
            for (int i = 0; i < d.CodeCount; i++)
            {
                result.Add(new DomainRecord() { Code = d.Value[i].ToString(), Description = d.Name[i] });
            }
            return result;
        }

        public IEnvelope GetBlocExtent(string blocNumber)
        {
            var fc = _featureWorkspace.OpenTable(ConfigurationManager.AppSettings["BlocTableName"]);
            IQueryFilter filter = new QueryFilter();
            filter.WhereClause = string.Format("NoBloc = '{0}'", blocNumber);
            var result = new List<BlocTBE>(fc.Map<BlocTBE>(filter));
            //Dim pEnv As IEnvelope = New EnvelopeClass()
            var envelopeTotal = new Envelope() as IEnvelope;
            result.ForEach(x =>
                               {
                                   IEnvelope envelope = x.Shape.Envelope;
                                   envelopeTotal.Union(envelope);
                               });
            return envelopeTotal;
        }
    }
}