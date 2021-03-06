﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CarShack.Domain.Customer;
using CarShack.Hypermedia.Customers;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;
using WebApiHypermediaExtensionsCore.Util.Repository;
using WebApiHypermediaExtensionsCore.WebApi;
using WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes;
using WebApiHypermediaExtensionsCore.WebApi.ExtensionMethods;

namespace CarShack.Controllers.Customers
{
    [Route("Customers/")]
    public class CustomersRootController : Controller
    {
        private readonly HypermediaCustomersRoot customersRoot;
        private readonly ICustomerRepository customerRepository;

        public CustomersRootController(HypermediaCustomersRoot customersRoot, ICustomerRepository customerRepository)
        {
            this.customersRoot = customersRoot;
            this.customerRepository = customerRepository;
        }

#region HypermediaObjects
        // Route to the HypermediaCustomersRoot. References to HypermediaCustomersRoot type will be resolved to this route.
        [HttpGetHypermediaObject("", typeof(HypermediaCustomersRoot))]
        public ActionResult GetRootDocument()
        {
            return Ok(customersRoot);
        }

        // Building Queries using the CreateQuery will link to this route.
        [HttpGetHypermediaObject("Query", typeof(HypermediaCustomerQueryResult))]
        public async Task<ActionResult> Query([FromQuery] CustomerQuery query)
        {
            if (query == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            var queryResult = await customerRepository.QueryAsync(query);
            var resultReferences = new List<HypermediaObjectReferenceBase>();
            foreach (var customer in queryResult.Entities)
            {
                resultReferences.Add(new HypermediaObjectReference(new HypermediaCustomer(customer)));
            }

            var result = new HypermediaCustomerQueryResult(resultReferences, queryResult.TotalCountOfEnties, query);
            var navigationQuerys = NavigationQuerysBuilder.Build(query, queryResult);
            result.AddNavigationQueries(navigationQuerys);
           
            return Ok(result);
        }
#endregion

#region Actions
        // Provides a link to the result Query.
        [HttpPostHypermediaAction("CreateQuery", typeof(HypermediaAction<CustomerQuery>))]
        public ActionResult NewQueryAction([SingleParameterBinder(typeof(CustomerQuery))] CustomerQuery query)
        {
            if (query == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            if (!customersRoot.CreateQueryAction.CanExecute())
            {
                return this.CanNotExecute();
            }

            // Will create a Location header with a URI to the result.
            return this.CreatedQuery(typeof(HypermediaCustomerQueryResult), query);
        }

        [HttpPostHypermediaAction("CreateCustomer", typeof(HypermediaFunction<CreateCustomerParameters, Task<Customer>>))]
        public async Task<ActionResult> NewCustomerAction([SingleParameterBinder(typeof(CreateCustomerParameters))] CreateCustomerParameters createCustomerParameters)
        {
            if (createCustomerParameters == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            var createdCustomer = await customersRoot.CreateCustomerAction.Execute(createCustomerParameters);

            // Will create a Location header with a URI to the result.
            return this.Created(new HypermediaCustomer(createdCustomer));
        }
#endregion

#region TypeRoutes
        // Provide tyoe information for Action parameters
        [HttpGetHypermediaActionParameterInfo("CreateCustomerParametersType", typeof(CreateCustomerParameters))]
        public ActionResult CreateCustomerParametersType()
        {
            var schema = JsonSchemaFactory.Generate(typeof(CreateCustomerParameters));
            return Ok(schema);
        }

        [HttpGetHypermediaActionParameterInfo("CustomerQueryType", typeof(CustomerQuery))]
        public ActionResult CustomerQueryType()
        {
            var schema = JsonSchemaFactory.Generate(typeof(CustomerQuery));

            return Ok(schema);
        }
#endregion
    }
}
