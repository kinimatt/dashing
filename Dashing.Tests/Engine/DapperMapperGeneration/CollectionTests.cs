﻿namespace Dashing.Tests.Engine.DapperMapperGeneration {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using Dashing.CodeGeneration;
    using Dashing.Configuration;
    using Dashing.Engine.DapperMapperGeneration;
    using Dashing.Engine.Dialects;
    using Dashing.Engine.DML;
    using Dashing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class CollectionTests {
        [Fact]
        public void SingleRowCollectionTestForJames() {
            var funcFac = GenerateSingleMapperWithFetch();
            var post1 = new Post { PostId = 1 };
            var comment1 = new Comment { CommentId = 1 };
            var blog1 = new Blog { BlogId = 1 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, blog1, comment1 });
            Assert.Equal(1, results[0].Comments.First().CommentId);
            Assert.Equal(1, results[0].Blog.BlogId);
        }

        [Fact]
        public void SingleCollectionWorks() {
            var funcFac = GenerateSingleMapper();
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, comment1 });
            func(new object[] { post1, comment2 });
            func(new object[] { post2, comment3 });
            Assert.Equal(1, results[0].Comments.First().CommentId);
            Assert.Equal(2, results[0].Comments.Last().CommentId);
            Assert.Equal(3, results[1].Comments.First().CommentId);
        }

        [Fact]
        public void SingleCollectionAwkwardObjectWorks() {
            var funcFac = GenerateSingleAwkwardMapper();
            var post1 = new PostWithoutCollectionInitializerInConstructor { PostWithoutCollectionInitializerInConstructorId = 1 };
            var post2 = new PostWithoutCollectionInitializerInConstructor { PostWithoutCollectionInitializerInConstructorId = 2 };
            var comment1 = new CommentTwo { CommentTwoId = 1 };
            var comment2 = new CommentTwo { CommentTwoId = 2 };
            var comment3 = new CommentTwo { CommentTwoId = 3 };
            PostWithoutCollectionInitializerInConstructor currentRoot = null;
            IList<PostWithoutCollectionInitializerInConstructor> results = new List<PostWithoutCollectionInitializerInConstructor>();
            var func = (Func<object[], PostWithoutCollectionInitializerInConstructor>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, comment1 });
            func(new object[] { post1, comment2 });
            func(new object[] { post2, comment3 });
            Assert.Equal(1, results[0].Comments.First().CommentTwoId);
            Assert.Equal(2, results[0].Comments.Last().CommentTwoId);
            Assert.Equal(3, results[1].Comments.First().CommentTwoId);
        }

        [Fact]
        public void ThenFetchWorks() {
            var funcFac = GenerateThenFetchMapper();
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var user1 = new User { UserId = 1 };
            var user2 = new User { UserId = 2 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results);
            func(new object[] { post1, comment1, user1 });
            func(new object[] { post1, comment2, user2 });
            func(new object[] { post2, comment3, user1 });
            Assert.Equal(1, results[0].Comments.First().User.UserId);
            Assert.Equal(2, results[0].Comments.Last().User.UserId);
            Assert.Equal(1, results[1].Comments.First().User.UserId);
        }

        private static Delegate GenerateThenFetchMapper() {
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).FetchMany(p => p.Comments).ThenFetch(c => c.User) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(GetMockCodeManager().Object, config);
            var func = mapper.GenerateCollectionMapper<Post>(result.FetchTree, false);
            return func.Item1;
        }

        [Fact]
        public void MultiCollectionWorks() {
            var funcFac = GenerateMultiMapper();

            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var comment1 = new Comment { CommentId = 1 };
            var comment2 = new Comment { CommentId = 2 };
            var comment3 = new Comment { CommentId = 3 };
            var postTag1 = new PostTag { PostTagId = 1 };
            var postTag2 = new PostTag { PostTagId = 2 };
            var postTag3 = new PostTag { PostTagId = 3 };
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            IDictionary<int, Comment> dict0 = new Dictionary<int, Comment>();
            var hashsetPair0 = new HashSet<Tuple<int, int>>();
            IDictionary<int, PostTag> dict1 = new Dictionary<int, PostTag>();
            var hashsetPair1 = new HashSet<Tuple<int, int>>();

            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results, dict0, hashsetPair0, dict1, hashsetPair1);
            func(new object[] { post1, comment1, postTag1 });
            func(new object[] { post1, comment2, postTag1 });
            func(new object[] { post2, comment3, postTag2 });
            func(new object[] { post2, comment3, postTag3 });

            Assert.Equal(1, results[0].Comments.First().CommentId);
            Assert.Equal(2, results[0].Comments.Last().CommentId);
            Assert.Equal(2, results[0].Comments.Count);

            Assert.Equal(3, results[1].Comments.First().CommentId);
            Assert.Equal(1, results[1].Comments.Count);

            Assert.Equal(1, results[0].Tags.First().PostTagId);
            Assert.Equal(1, results[0].Tags.Count);

            Assert.Equal(2, results[1].Tags.First().PostTagId);
            Assert.Equal(3, results[1].Tags.Last().PostTagId);
            Assert.Equal(2, results[1].Tags.Count);
        }

        [Fact]
        public void MultipleManyToManyFetchingWorks() {
            // setup the factory
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).FetchMany(p => p.Tags).ThenFetch(p => p.ElTag).FetchMany(p => p.DeletedTags).ThenFetch(t => t.ElTag) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);
            var mapper = new DapperMapperGenerator(GetMockCodeManager().Object, config);
            var funcFac = mapper.GenerateMultiCollectionMapper<Post>(result.FetchTree, false).Item1;

            // setup the scenario
            var post1 = new Post { PostId = 1 };
            var tag1 = new Tag { TagId = 1 };
            var tag2 = new Tag { TagId = 2 };
            var tag3 = new Tag { TagId = 3 };
            var postTag1 = new PostTag { PostTagId = 1 };
            var postTag2 = new PostTag { PostTagId = 2 };
            var postTag3 = new PostTag { PostTagId = 3 };
            
            // act
            Post currentRoot = null;
            IList<Post> results = new List<Post>();
            var dict0 = new Dictionary<int, PostTag>();
            var hashsetPair0 = new HashSet<Tuple<int, int>>();
            var dict1 = new Dictionary<int, PostTag>();
            var hashsetPair1 = new HashSet<Tuple<int, int>>();

            var func = (Func<object[], Post>)funcFac.DynamicInvoke(currentRoot, results, dict0, hashsetPair0, dict1, hashsetPair1);
            func(new object[] { post1, postTag2, tag2, postTag1, tag1 });
            func(new object[] { post1, postTag3, tag3, postTag1, tag1 });

            Assert.Equal(1, results.Count);
            Assert.Equal(1, results[0].Tags.Count);
            Assert.Equal(1, results[0].Tags[0].PostTagId);
            Assert.Equal(2, results[0].DeletedTags.Count);
            Assert.Equal(2, results[0].DeletedTags[0].PostTagId);
            Assert.Equal(3, results[0].DeletedTags[1].PostTagId);
        }

        [Fact]
        public void NestedMultipleOneToManyFetchingWorks() {
            // setup the factory
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Blog>(new Mock<ISelectQueryExecutor>().Object).FetchMany(b => b.Posts).ThenFetchMany(p => p.Tags).ThenFetch(t => t.ElTag).FetchMany(b => b.Posts).ThenFetchMany(p => p.DeletedTags).ThenFetch(t => t.ElTag).FetchMany(p => p.Posts).ThenFetch(p => p.Author) as SelectQuery<Blog>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);
            var mapper = new DapperMapperGenerator(GetMockCodeManager().Object, config);
            var funcFac = mapper.GenerateMultiCollectionMapper<Blog>(result.FetchTree, false).Item1;

            // setup the scenario
            var blog1 = new Blog { BlogId = 1 };
            var blog2 = new Blog { BlogId = 2 };
            var post1 = new Post { PostId = 1 };
            var post2 = new Post { PostId = 2 };
            var post3 = new Post { PostId = 3 };
            var posttag1 = new PostTag { PostTagId = 1 };
            var posttag2 = new PostTag { PostTagId = 2 };
            var posttag3 = new PostTag { PostTagId = 3 };
            var tag1 = new Tag { TagId = 1 };
            var tag2 = new Tag { TagId = 2 };
            var tag3 = new Tag { TagId = 3 };
            var tag4 = new Tag { TagId = 4 };
            var delPostTag1 = new PostTag { PostTagId = 3 };
            var delPostTag2 = new PostTag { PostTagId = 4 };
            var delPostTag3 = new PostTag { PostTagId = 5 };
            var author1 = new User { UserId = 1 };
            var author2 = new User { UserId = 2 };

            // act
            Blog currentRoot = null;
            IList<Blog> results = new List<Blog>();
            var dict0 = new Dictionary<int, Post>();
            var hashsetPair0 = new HashSet<Tuple<int, int>>();
            var dict1 = new Dictionary<int, PostTag>();
            var hashsetPair1 = new HashSet<Tuple<int, int>>();
            var dict2 = new Dictionary<int, PostTag>();
            var hashsetPair2 = new HashSet<Tuple<int, int>>();

            var func = (Func<object[], Blog>)funcFac.DynamicInvoke(currentRoot, results, dict0, hashsetPair0, dict1, hashsetPair1, dict2, hashsetPair2);
            func(new object[] { blog1, post1, author1, null, null, posttag1, tag1 });
            func(new object[] { blog1, post1, author1, null, null, posttag2, tag2 });
            func(new object[] { blog1, post2, author2, delPostTag1, tag3, null, null });
            func(new object[] { blog1, post2, author2, delPostTag2, tag4, null, null });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag1, tag1 });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag2, tag2 });
            func(new object[] { blog2, post3, author1, delPostTag1, tag3, posttag3, tag3 });
            func(new object[] { blog2, post3, author1, delPostTag2, tag4, posttag3, tag3 });
            func(new object[] { blog2, post3, author1, delPostTag3, tag4, posttag3, tag3 });

            Assert.Equal(2, results.Count);
            Assert.Equal(2, results[0].Posts.Count);
            Assert.Equal(1, results[0].Posts[0].Author.UserId);
            Assert.True(results[0].Posts[0].DeletedTags == null || !results[0].Posts[0].DeletedTags.Any());
            Assert.Equal(2, results[0].Posts[0].Tags.Count);
            Assert.Equal(1, results[0].Posts[0].Tags[0].PostTagId);
            Assert.Equal(1, results[0].Posts[0].Tags[0].ElTag.TagId);
            Assert.Equal(2, results[0].Posts[0].Tags[1].PostTagId);
            Assert.Equal(2, results[0].Posts[0].Tags[1].ElTag.TagId);
            Assert.Equal(1, results[1].Posts.Count);
            Assert.Equal(1, results[1].Posts[0].Author.UserId);
            Assert.Equal(3, results[1].Posts[0].DeletedTags.Count);
            Assert.Equal(3, results[1].Posts[0].Tags.Count);
            Assert.Equal(3, results[1].Posts[0].DeletedTags[0].PostTagId);
            Assert.Equal(4, results[1].Posts[0].DeletedTags[1].PostTagId);
            Assert.Equal(5, results[1].Posts[0].DeletedTags[2].PostTagId);
            Assert.Equal(3, results[1].Posts[0].DeletedTags[0].ElTag.TagId);
            Assert.Equal(4, results[1].Posts[0].DeletedTags[1].ElTag.TagId);
            Assert.Equal(4, results[1].Posts[0].DeletedTags[2].ElTag.TagId);
            Assert.Equal(1, results[1].Posts[0].Tags[0].PostTagId);
            Assert.Equal(2, results[1].Posts[0].Tags[1].PostTagId);
            Assert.Equal(3, results[1].Posts[0].Tags[2].PostTagId);
            Assert.Equal(1, results[1].Posts[0].Tags[0].ElTag.TagId);
            Assert.Equal(2, results[1].Posts[0].Tags[1].ElTag.TagId);
            Assert.Equal(3, results[1].Posts[0].Tags[2].ElTag.TagId);
        }

        private static Delegate GenerateMultiMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).Fetch(p => p.Comments).Fetch(p => p.Tags) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(GetMockCodeManager().Object, config);
            var func = mapper.GenerateMultiCollectionMapper<Post>(result.FetchTree, false);
            return func.Item1;
        }

        private static Delegate GenerateSingleMapperWithFetch() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).Fetch(p => p.Comments).Fetch(p => p.Blog) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(GetMockCodeManager().Object, config);
            var func = mapper.GenerateCollectionMapper<Post>(result.FetchTree, false);
            return func.Item1;
        }

        private static Delegate GenerateSingleMapper() {
            var config = new CustomConfig();
            var selectQuery = new SelectQuery<Post>(new Mock<ISelectQueryExecutor>().Object).Fetch(p => p.Comments) as SelectQuery<Post>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(GetMockCodeManager().Object, config);
            var func = mapper.GenerateCollectionMapper<Post>(result.FetchTree, false);
            return func.Item1;
        }

        private static Delegate GenerateSingleAwkwardMapper() {
            var config = new CustomConfig();
            var selectQuery =
                new SelectQuery<PostWithoutCollectionInitializerInConstructor>(new Mock<ISelectQueryExecutor>().Object).Fetch(p => p.Comments) as
                SelectQuery<PostWithoutCollectionInitializerInConstructor>;
            var writer = new SelectWriter(new SqlServer2012Dialect(), config);
            var result = writer.GenerateSql(selectQuery);

            var mapper = new DapperMapperGenerator(GetMockCodeManager().Object, config);
            var func = mapper.GenerateCollectionMapper<PostWithoutCollectionInitializerInConstructor>(result.FetchTree, false);
            return func.Item1;
        }

        private static Mock<IGeneratedCodeManager> GetMockCodeManager() {
            var mockCodeManager = new Mock<IGeneratedCodeManager>(MockBehavior.Strict);
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Post))).Returns(typeof(Post));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Blog))).Returns(typeof(Blog));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Comment))).Returns(typeof(Comment));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(User))).Returns(typeof(User));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(Tag))).Returns(typeof(Tag));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(PostTag))).Returns(typeof(PostTag));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(PostWithoutCollectionInitializerInConstructor)))
                           .Returns(typeof(PostWithoutCollectionInitializerInConstructor));
            mockCodeManager.Setup(c => c.GetForeignKeyType(typeof(CommentTwo))).Returns(typeof(CommentTwo));

            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Post)))).Returns(typeof(Post));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Blog)))).Returns(typeof(Blog));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Comment)))).Returns(typeof(Comment));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(User)))).Returns(typeof(User));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(Tag)))).Returns(typeof(Tag));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(PostTag)))).Returns(typeof(PostTag));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(PostWithoutCollectionInitializerInConstructor))))
                           .Returns(typeof(PostWithoutCollectionInitializerInConstructor));
            mockCodeManager.Setup(c => c.GetForeignKeyType(It.Is<Type>(t => t == typeof(CommentTwo)))).Returns(typeof(CommentTwo));
            return mockCodeManager;
        }

        private class CustomConfig : DefaultConfiguration {
            public CustomConfig()
                : base(new ConnectionStringSettings("Default", string.Empty, "System.Data.SqlClient")) {
                this.AddNamespaceOf<Post>();
                this.Add<PostWithoutCollectionInitializerInConstructor>();
                this.Add<CommentTwo>();
            }
        }

        private class PostWithoutCollectionInitializerInConstructor {
            public virtual int PostWithoutCollectionInitializerInConstructorId { get; set; }

            public virtual ICollection<CommentTwo> Comments { get; set; }
        }

        private class CommentTwo {
            public virtual int CommentTwoId { get; set; }

            public virtual PostWithoutCollectionInitializerInConstructor PostWithoutCollectionInitializerInConstructor { get; set; }
        }
    }
}