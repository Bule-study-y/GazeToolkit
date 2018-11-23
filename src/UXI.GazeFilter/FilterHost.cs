﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UXI.GazeToolkit.Serialization;
using UXI.GazeToolkit.Serialization.Converters;

namespace UXI.GazeFilter
{
    public abstract class FilterHost
    {
        private readonly StringWriter _help;
        private readonly Parser _commandLineParser;

        public FilterHost(IEnumerable<IFilter> filters)
        {
            if (filters == null || filters.Any() == false)
            {
                throw new ArgumentNullException(nameof(filters), "No filter specified");
            }

            _help = new StringWriter();
            _commandLineParser = new Parser(s =>
            {
                s.CaseSensitive = false;
                s.CaseInsensitiveEnumValues = true;
                s.ParsingCulture = CultureInfo.GetCultureInfo("en-US");
                s.HelpWriter = _help;
            });

            Filters = filters.ToDictionary(f => f.OptionsType);

            Context = new FilterContext();           
        }

        
        public FilterHost(Action<FilterContext> configure, IEnumerable<IFilter> filters)
            : this(filters)
        {
            configure?.Invoke(Context);
        }


        public Dictionary<Type, IFilter> Filters { get; }


        public FilterContext Context { get; }


        public int Execute(string[] args)
        {
            BaseOptions options;
            IFilter filter;

            if (TryParseFilterOptions(_commandLineParser, args, out options)
                && Filters.TryGetValue(options.GetType(), out filter))
            {
//#if DEBUG
//                Console.Error.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(options, Newtonsoft.Json.Formatting.Indented, new StringEnumConverter(false)));
//#endif
                Configure(options);

                using (var cts = new CancellationTokenSource())
                {
                    var execution = ExecuteAsync(filter, options, cts.Token);

                    Console.CancelKeyPress += (_, __) => cts.Cancel();

                    execution.Wait();
                }

                return 0;
            }
            else
            {
                Console.Error.WriteLine(_help.ToString());

                return 1;
            }
        }


        protected abstract bool TryParseFilterOptions(Parser parser, string[] args, out BaseOptions options);
        

        private Task ExecuteAsync(IFilter filter, BaseOptions options, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            var io = new DataIO(Context.Formats);

            Context.IO = io;

            filter.Initialize(options, Context);

            io.ReadInput(options, filter.InputType, Context.Serialization)
              .SubscribeOn(NewThreadScheduler.Default)
              .Process(filter, options)
              .WriteOutput(io, options, filter.OutputType, Context.Serialization)
              .Subscribe(_ => { }, e => tcs.TrySetException(e), () => tcs.TrySetResult(true));

            return tcs.Task;
        }


        private void Configure(BaseOptions options)
        {
            Context.Serialization.TimestampConverter = TimestampStringConverterResolver.Default.Resolve(options.TimestampFormat);
            Context.Serialization.TimestampFieldName = options.TimestampFieldName;
        }
    }
}