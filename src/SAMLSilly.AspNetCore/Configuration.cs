﻿using SAMLSilly.Config;
using System;

namespace SAMLSilly.AspNet
{
    public class ConfigurationFactory
    {
        private Saml2Configuration configuration;
        public Saml2Configuration Configuration
        {
            get {
                return configuration ?? (configuration = GetConfiguration());
            }
        }

        private Saml2Configuration GetConfiguration()
        {
            return ((IConfigurationProvider)Activator.CreateInstance(Type.GetType(FetcherType))).GetConfiguration();
        }

        private string fetcherType;
        public string FetcherType
        {
            get
            {
                return fetcherType ?? (fetcherType = GetFetcherType());
            }
            set
            {
                configuration = null;
                fetcherType = value;
            }
        }

        private string GetFetcherType()
        {
            return System.Configuration.ConfigurationManager.AppSettings["saml2:configurationReaderType"] ?? "SAMLSilly.AspNetCore.WebConfigConfigurationReader";
        }

        private static readonly ConfigurationFactory instance = new ConfigurationFactory();
        public static ConfigurationFactory Instance { get { return instance;  } }
    }
}
