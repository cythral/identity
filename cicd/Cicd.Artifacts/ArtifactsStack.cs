using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.S3;

using Constructs;

using EcrLifecycleRule = Amazon.CDK.AWS.ECR.LifecycleRule;

namespace Brighid.Identity.Artifacts
{
    /// <summary>
    /// Stack that contains repositories for storing artifacts.
    /// </summary>
    public class ArtifactsStack : Stack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactsStack" /> class.
        /// </summary>
        /// <param name="scope">The scope to create this artifacts stack in.</param>
        /// <param name="id">The ID of the Artifacts Stack.</param>
        /// <param name="props">The props for the Artifacts Stack.</param>
        public ArtifactsStack(Construct scope, string id, IStackProps? props = null)
            : base(scope, id, props)
        {
            AddRepository();
            AddBucket();
            AddDns();
        }

        private static CfnRecordSetGroup.RecordSetProperty CreateDnsAliasRecord(string domainName, string type)
        {
            return new CfnRecordSetGroup.RecordSetProperty
            {
                Name = domainName,
                Type = type,
                AliasTarget = new CfnRecordSetGroup.AliasTargetProperty
                {
                    HostedZoneId = Fn.ImportValue("cfn-gateway:LoadBalancerCanonicalHostedZoneId"),
                    DnsName = Fn.ImportValue("cfn-gateway:LoadBalancerDnsName"),
                },
            };
        }

        private void AddBucket()
        {
            var bucket = new Bucket(this, "Bucket");
            bucket.ApplyRemovalPolicy(RemovalPolicy.DESTROY);
            bucket.AddToResourcePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[] { "s3:*Object" },
                Resources = new[] { bucket.BucketArn, $"{bucket.BucketArn}/*" },
                Principals = new[]
                {
                    new AccountPrincipal(Fn.Ref("AWS::AccountId")),
                    new ArnPrincipal(Fn.ImportValue("cfn-metadata:DevAgentRoleArn")),
                    new ArnPrincipal(Fn.ImportValue("cfn-metadata:ProdAgentRoleArn")),
                },
            }));

            _ = new CfnOutput(this, "BucketName", new CfnOutputProps
            {
                Value = bucket.BucketName,
                Description = "Name of the Artifacts Bucket for Brighid Identity.",
            });
        }

        private void AddRepository()
        {
            var repository = new Repository(this, "ImageRepository", new RepositoryProps
            {
                RepositoryName = "brighid/identity",
                ImageScanOnPush = true,
                LifecycleRules = new[]
                {
                    new EcrLifecycleRule
                    {
                        Description = "Protect prod-tagged images.",
                        RulePriority = 1,
                        TagStatus = TagStatus.TAGGED,
                        TagPrefixList = new[] { "production" },
                        MaxImageCount = 1,
                    },
                    new EcrLifecycleRule
                    {
                        Description = "Protect dev-tagged images.",
                        RulePriority = 2,
                        TagStatus = TagStatus.TAGGED,
                        TagPrefixList = new[] { "development" },
                        MaxImageCount = 1,
                    },
                    new EcrLifecycleRule
                    {
                        Description = "Keep last 3 images not tagged with dev or prod",
                        RulePriority = 3,
                        TagStatus = TagStatus.ANY,
                        MaxImageCount = 3,
                    },
                },
            });

            repository.AddToResourcePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Effect = Effect.ALLOW,
                Actions = new[]
                {
                    "ecr:GetAuthorizationToken",
                    "ecr:GetDownloadUrlForLayer",
                    "ecr:BatchGetImage",
                    "ecr:BatchCheckLayerAvailability",
                    "ecr:ListImages",
                    "ecr:PutImage", // For re-tagging images only
                },
                Principals = new[]
                {
                    new AccountPrincipal(Fn.Ref("AWS::AccountId")),
                    new AccountPrincipal(Fn.ImportValue("cfn-metadata:DevAccountId")),
                    new AccountPrincipal(Fn.ImportValue("cfn-metadata:ProdAccountId")),
                },
            }));

            repository.ApplyRemovalPolicy(RemovalPolicy.DESTROY);

            _ = new CfnOutput(this, "ImageRepositoryUri", new CfnOutputProps
            {
                Value = repository.RepositoryUri,
                Description = "URI of the container image repository for Brighid Identity.",
            });
        }

        private void AddDns()
        {
            _ = new CfnRecordSetGroup(this, "DevDnsRecords", new CfnRecordSetGroupProps
            {
                HostedZoneId = Fn.ImportValue("cfn-dns:HostedZoneId"),
                RecordSets = new[]
                {
                    CreateDnsAliasRecord("identity.dev.brigh.id", "A"),
                    CreateDnsAliasRecord("identity.dev.brigh.id", "AAAA"),
                },
            });

            _ = new CfnRecordSetGroup(this, "ProdDnsRecords", new CfnRecordSetGroupProps
            {
                HostedZoneId = Fn.ImportValue("cfn-dns:HostedZoneId"),
                RecordSets = new[]
                {
                    CreateDnsAliasRecord("identity.brigh.id", "A"),
                    CreateDnsAliasRecord("identity.brigh.id", "AAAA"),
                },
            });
        }
    }
}
