﻿namespace Dashing.Engine.DML {
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
#if COREFX
    using System.Reflection;
#endif

    using Dashing.Configuration;

    public class FetchTreeParser {
        private readonly IConfiguration configuration;

        public FetchTreeParser(IConfiguration configuration) {
            this.configuration = configuration;
        }

        public FetchNode GetFetchTree<T>(SelectQuery<T> selectQuery, out int aliasCounter, out int numberCollectionFetches) where T : class, new() {
            FetchNode rootNode = null;
            numberCollectionFetches = 0;
            aliasCounter = 0;

            if (selectQuery.HasFetches()) {
                // now we go through the fetches and generate the tree structure
                rootNode = new FetchNode { Alias = "t" };
                foreach (var fetch in selectQuery.Fetches) {
                    var lambda = fetch as LambdaExpression;
                    if (lambda != null) {
                        var expr = lambda.Body as MemberExpression;
                        var currentNode = rootNode;
                        var entityNames = new Stack<string>();

                        // TODO Change this so that algorithm only goes through tree once
                        // We go through the fetch expression (backwards)
                        while (expr != null) {
                            entityNames.Push(expr.Member.Name);
                            expr = expr.Expression as MemberExpression;
                        }

                        // Now go through the expression forwards adding in nodes where needed
                        this.AddPropertiesToFetchTree<T>(ref aliasCounter, ref numberCollectionFetches, entityNames, currentNode, rootNode);
                    }
                }

                // now iterate through the collection fetches
                foreach (var collectionFetch in selectQuery.CollectionFetches) {
                    var entityNames = new Stack<string>();
                    var currentNode = rootNode;

                    // start at the top of the stack, pop the expression off and do as above
                    for (var i = collectionFetch.Value.Count - 1; i >= 0; i--) {
                        var lambdaExpr = collectionFetch.Value[i] as LambdaExpression;
                        if (lambdaExpr != null) {
                            var expr = lambdaExpr.Body as MemberExpression;
                            while (expr != null) {
                                entityNames.Push(expr.Member.Name);
                                expr = expr.Expression as MemberExpression;
                            }
                        }
                    }

                    // add in the initial fetch many
                    var fetchManyLambda = collectionFetch.Key as LambdaExpression;
                    if (fetchManyLambda != null) {
                        var expr = fetchManyLambda.Body as MemberExpression;
                        while (expr != null) {
                            entityNames.Push(expr.Member.Name);
                            expr = expr.Expression as MemberExpression;
                        }
                    }

                    this.AddPropertiesToFetchTree<T>(ref aliasCounter, ref numberCollectionFetches, entityNames, currentNode, rootNode);
                }
            }

            return rootNode;
        }

        private void AddPropertiesToFetchTree<T>(
            ref int aliasCounter,
            ref int numberCollectionFetches,
            Stack<string> entityNames,
            FetchNode currentNode,
            FetchNode rootNode) {
            while (entityNames.Count > 0) {
                var propName = entityNames.Pop();

                // don't add duplicates
                if (!currentNode.Children.ContainsKey(propName)) {
                    var column =
                        this.configuration.GetMap(
                            currentNode == rootNode
                                ? typeof(T)
                                : (currentNode.Column.Relationship == RelationshipType.OneToMany
                                       ? currentNode.Column.Type.GetGenericArguments().First()
                                       : currentNode.Column.Type)).Columns[propName];
                    if (column.IsIgnored) {
                        //TODO we should probably warn at this point
                        continue;
                    }

                    if (column.Relationship == RelationshipType.OneToMany) {
                        ++numberCollectionFetches;
                    }

                    // add to tree
                    var node = new FetchNode { Parent = currentNode, Column = column, Alias = "t_" + ++aliasCounter, IsFetched = true };
                    if (column.Relationship == RelationshipType.OneToMany) {
                        // go through and increase the number of contained collections in each parent node
                        var parent = node.Parent;
                        while (parent != null) {
                            ++parent.ContainedCollectionfetchesCount;
                            parent = parent.Parent;
                        }
                    }

                    // insert the node in the correct order (i.e. respecting the FetchId and then all other things that depend on this
                    // should be constant
                    if (currentNode.Children.Any()) {
                        var i = 0;
                        var inserted = false;
                        foreach (var child in currentNode.Children) {
                            if (child.Value.Column.FetchId > column.FetchId) {
                                currentNode.Children.Insert(i, new KeyValuePair<string, FetchNode>(propName, node));
                                inserted = true;
                                break;
                            }

                            i++;
                        }

                        if (!inserted) {
                            currentNode.Children.Add(propName, node);
                        }
                    }
                    else {
                        currentNode.Children.Add(propName, node);
                    }

                    currentNode = node;
                }
                else {
                    currentNode = currentNode.Children[propName];
                }
            }
        }
    }
}