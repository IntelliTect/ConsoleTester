﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IntelliTect.TestTools.TestFramework
{
    public class TestBuilder
    {
        // DON'T FORGET TO UNIT TEST ONCE YOU'RE BACK ON YOUR NORMAL MACHINE
        public TestBuilder AddTestBlock<T>()
        {
            TestBlockTypes.Add(typeof(T));
            return this;
        }

        // Probably change this to a factory pattern?
        public TestBuilder AddData<T>()
        {
            AddDataToBag(default(T));
            return this;
        }

        public TestBuilder AddData(object data)
        {
            AddDataToBag(data);
            return this;
        }

        public TestBuilder AddData(params object[] data)
        {
            foreach (var d in data)
            {
                AddDataToBag(d);
            }
            return this;
        }

        // Should this return a new type?
        // Also... should we call it something like "Validate" or "Build"? "Build" seems more consistent with the pattern.
        // ALSO... can we avoid iterating over the arguments twice? Maybe use this method to pair up test blocks with the arguments? See example below...
        public TestBuilder ValidateTest()
        {
            // Need to build a temporary list to get return values...
            Type[] existingOrReturnedParams = new Type[0];
            Type[] askedForParams = new Type[0];

            foreach (Type tbt in TestBlockTypes)
            {
                var tb = new TestBlock();
                // Gather constructor arguments
                tb.Constructor = tbt.GetConstructors().First();

                // Update below method. This will fail in certain circumstances when we're relying on a return value
                // Maybe have two separate methods: ValidateAndFetch, and DoesItemExist?
                tb.ConstructorArguments = ValidateAndFetchTestBlockParameters(tb.Constructor.GetParameters());

                // Gather Execute method arguments
                tb.ExecuteMethod = tbt.GetMethod("Execute");

                if (tb.ExecuteMethod.ReturnType != typeof(void))
                    existingOrReturnedParams = existingOrReturnedParams.Concat(new Type[] { tb.ExecuteMethod.ReturnType }).ToArray();

                foreach(var c in tb.Constructor.GetParameters())
                {
                    askedForParams = askedForParams.Concat(new Type[] { c.ParameterType }).ToArray();
                }

                foreach(var p in tb.ExecuteMethod.GetParameters())
                {
                    askedForParams = askedForParams.Concat(new Type[] { p.ParameterType }).ToArray();
                }

                // Update below method. This will fail in certain circumstances when we're relying on a return value
                tb.ExecuteArguments = ValidateAndFetchTestBlockParameters(tb.ExecuteMethod.GetParameters());
            }

            foreach (var d in Data)
            {
                existingOrReturnedParams = existingOrReturnedParams.Concat(new Type[] { d.GetType() }).ToArray();
            }

            // Verify everything matches here. As with fetching, getting the drivers is going to rely on comparing the IWebDriver interface to the implemented interfaces
            // Maybe remove all other objects as the types check out?

            return this;
        }

        public void ExecuteTestCase()
        {
            foreach (var tb in TestBlocksAndArguments)
            {
                ConstructorInfo ctor = tb.ExecuteMethod.DeclaringType.GetConstructors().First();

                // Create instance of test block
                object tbInstance = ctor.Invoke(tb.ConstructorArguments);

                Log.Info($"Starting test block {tb}");
                Log.Info($"Using additional inputs {JsonConvert.SerializeObject(tb.ExecuteArguments, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })}");

                Stopwatch sw = new Stopwatch();
                sw.Start();
                object result = tb.ExecuteMethod.Invoke(tbInstance, tb.ExecuteArguments);
                sw.Stop();

                Log.Info($"Time for test block to execute: {sw.Elapsed}");
                if (result != null)
                {
                    Log.Info($"Test block returned... {JsonConvert.SerializeObject(result, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })}");
                    AddDataToBag(result);
                }
            }
        }

        public void UnsafeExecuteTestCase()
        {
            // Check for non-existent types here?

            // Execute test blocks
            foreach (Type tb in TestBlockTypes)
            {
                // Get constructor and arguments
                Log.Info($"Gathering constructor arguments for test block {tb}...");
                ConstructorInfo[] ctors = tb.GetConstructors();
                ParameterInfo[] ctorArgs = ctors.First().GetParameters();
                object[] args = ValidateAndFetchTestBlockParameters(ctorArgs);

                // Create instance of test block
                object tbInstance = ctors.First().Invoke(args);

                // Gather Execute method arguments
                Log.Info($"Gathering properties for test block {tb}...");
                MethodInfo execute = tb.GetMethod("Execute");
                ParameterInfo[] argInfo = execute.GetParameters();
                args = ValidateAndFetchTestBlockParameters(argInfo);

                Log.Info($"Starting test block {tb.Name}");
                Log.Info($"Using additional inputs {JsonConvert.SerializeObject(args, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })}");

                Stopwatch sw = new Stopwatch();
                sw.Start();
                object result = execute.Invoke(tbInstance, args);
                sw.Stop();

                Log.Info($"Time for test block to execute: {sw.Elapsed}");
                if (result != null)
                {
                    Log.Info($"Test block returned... {JsonConvert.SerializeObject(result, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })}");
                    AddDataToBag(result);
                }
            }
        }

        private bool ItemExistsInBag()
        {

        }

        private object[] ValidateAndFetchTestBlockParameters(ParameterInfo[] parameters)
        {
            object[] args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var t = parameters[i].ParameterType;
                if (!TryGetItemFromBag(t, out object p))
                {
                    // Check for factories once that's implemented
                    string message = $"...expected an object of type {t.GetType()}, however none could be found in the item bag.";
                    Log.Info(message);
                    throw new ArgumentException(message);
                }
                args[i] = p;
            }
            Log.Info("Found all required items.");
            return args;
        }

        private void AddDataToBag(params object[] data)
        {
            foreach (var d in data)
            {
                var existingType = Data.SingleOrDefault(f => f.GetType() == d.GetType());
                if (existingType != null)
                {
                    Data.Remove(existingType);
                }
                Data.Add(d);
            }
        }

        

        private bool TryGetItemFromBag(Type typeToFind, out object data)
        {
            data = Data.SingleOrDefault(d => d.GetType() == typeToFind);
            if (data == null)
            {
                // Probably shouldn't do below as it breaks validation
                data = Data.SingleOrDefault(d => d.GetType().BaseType == typeToFind);
                if (data == null)
                {
                    // This will produce unexpected results if we load up two different browser types. It will grab whatever is first.
                    foreach (var d in Data)
                    {
                        Type[] interfaces = d.GetType().GetInterfaces();
                        if (interfaces.Length > 0 && interfaces.Contains(typeToFind))
                        {
                            data = d;
                            break;
                        }
                    }
                }
            }

            return data != null ? true : false;
        }

        private List<Type> TestBlockTypes { get; set; } = new List<Type>();
        private List<object> Data { get; set; } = new List<object>();
        private List<TestBlock> TestBlocksAndArguments { get; set; }
        private Log Log { get; set; } = new Log();
    }

    public class TestBlock
    {
        public ConstructorInfo Constructor { get; set; }
        public object[] ConstructorArguments { get; set; }
        // Probably need to add class properties
        public object[] ExecuteArguments { get; set; }
        public MethodInfo ExecuteMethod { get; set; }
    }
}
