﻿using MyLab.Search.EsAdapter;

namespace MyLab.Search.Indexer.Tools
{
    class MqCaseOptionsValidator
    {
        private readonly IndexerOptions _options;
        private readonly ElasticsearchOptions _esOptions;

        public MqCaseOptionsValidator(IndexerOptions options, ElasticsearchOptions esOptions)
        {
            _options = options;
            _esOptions = esOptions;
        }

        public void Validate()
        {
            OptionsValidatorTools.CheckEs(_esOptions);

            if (_options.Jobs != null)
            {
                foreach (var jobOptions in _options.Jobs)
                {
                    OptionsValidatorTools.CheckId(jobOptions);
                    OptionsValidatorTools.ThrowNotDefined(jobOptions, o => o.EsIndex);
                    OptionsValidatorTools.ThrowNotDefined(jobOptions, o => o.NewUpdatesStrategy);

                    if (jobOptions.NewUpdatesStrategy == NewUpdatesStrategy.Update)
                        OptionsValidatorTools.ThrowNotDefined(jobOptions, o => o.LastChangeProperty);

                    if (jobOptions.EnablePaging)
                        OptionsValidatorTools.ThrowNotDefined(jobOptions, o => o.PageSize);
                }
            }
        }
    }

    class DbCaseOptionsValidator
    {
        private readonly IndexerOptions _options;
        private readonly IndexerDbOptions _dbOptions;
        private readonly ElasticsearchOptions _esOptions;

        public DbCaseOptionsValidator(IndexerOptions options, IndexerDbOptions dbOptions, ElasticsearchOptions esOptions)
        {
            _options = options;
            _dbOptions = dbOptions;
            _esOptions = esOptions;
        }

        public void Validate()
        {
            OptionsValidatorTools.CheckEs(_esOptions);
            OptionsValidatorTools.ThrowNotDefined(_dbOptions, o => o.Provider);

            if (_options.Jobs != null)
            {
                foreach (var jobOptions in _options.Jobs)
                {
                    OptionsValidatorTools.CheckId(jobOptions);
                    OptionsValidatorTools.ThrowNotDefined(jobOptions, o => o.EsIndex);
                    OptionsValidatorTools.ThrowNotDefined(jobOptions, o => o.NewUpdatesStrategy);

                    if (jobOptions.NewUpdatesStrategy == NewUpdatesStrategy.Update)
                        OptionsValidatorTools.ThrowNotDefined(jobOptions, o => o.LastChangeProperty);

                    if (jobOptions.EnablePaging)
                        OptionsValidatorTools.ThrowNotDefined(jobOptions, o => o.PageSize);
                }
            }

        }
    }
}