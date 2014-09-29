﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.IntegrationTests.SqlServer {
    using Dashing.IntegrationTests.SqlServer.Fixtures;
    using Dashing.IntegrationTests.TestDomain;

    using Xunit;

    public class AsyncTests : IUseFixture<SqlServerFixture> {
        private SqlServerFixture fixture;

        [Fact]
        public async void GetByIdWorks() {
            var post = await this.fixture.Session.GetAsync<Post, int>(1);
            Assert.Equal(1, post.PostId);
        }

        [Fact]
        public async void QueryWorks() {
            var posts = await this.fixture.Session.Query<Post>().ToListAsync();
            Assert.Equal(20, posts.Count());
        }

        public void SetFixture(SqlServerFixture data) {
            this.fixture = data;
        }
    }
}
