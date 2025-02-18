using MBaileyDesignsDomain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MbaileyDesignsPersistence
{
    public class Seed
    {
        public static async Task SeedData (PostgresDataContext context)
        {
            bool hasUsers = await context.Users.AnyAsync();
            if (!hasUsers) 
            {
                var users = new List<User>()
                {
                    new User
                    {
                        Id = new Random().Next(1,9999999),
                        FirstName = "Matthew",
                        LastName = "Bailey",
                        Email = "matthewpbaileydesigns@gmail.com",
                        Password = "8Dapgm372!",
                        IsDeleted = false,
                        RefreshToken = null
                    }
                };

                await context.Users.AddRangeAsync(users);
            }

            bool hasBlogPosts = await context.BlogPosts.AnyAsync();
            if (!hasBlogPosts) 
            {
                var blogPosts = new List<BlogPost>()
                {
                    new BlogPost
                    {
                        Id = new Random().Next(1,9999999),
                        DatePosted=DateTime.UtcNow.AddDays(1).ToString("MM/dd/yyyy"),
                        DatePostedIso=DateTime.UtcNow,
                        Title="The difficulty of marketting yourself and maintaining a positive outlook as an autistic developer when you are unemployed",
                        Content="Help",
                        IsDeleted=false,
                        PostCategories=new List<BlogCategory>()
                        {
                            new BlogCategory(){
                                Id = new Random().Next(1, 9999999),
                                Title="Soft Skills",
                                IsDeleted=false
                            }
                        },
                        PostComments=new List<BlogComment>(),
                    },
                    new BlogPost
                    {
                        Id = new Random().Next(1,9999999),
                        DatePosted=DateTime.UtcNow.AddDays(2).ToString("MM/dd/yyyy"),
                        DatePostedIso=DateTime.UtcNow,
                        Title="Eh, yeah. AI is just another hype train. Sorry",
                        Content="\"I'm afraid I cannot do that dave...\" A chilling and albeit terrifying line from Stanly Kubric's classic adaptation of the visionary novel 2001: a Space Odessy",
                        IsDeleted=false,
                        PostComments=new List<BlogComment>(),
                        PostCategories=new List<BlogCategory>()
                        {
                            new BlogCategory(){
                                Id = new Random().Next(1, 9999999),
                                Title="AI",
                                IsDeleted=false
                            }
                        },
                    },
                    new BlogPost
                    {
                        Id = new Random().Next(1,9999999),
                        DatePosted=DateTime.UtcNow.AddDays(2).ToString("MM/dd/yyyy"),
                        DatePostedIso=DateTime.UtcNow,
                        Title="But I know javascript! Is that enough?",
                        Content="This honestly is kind of a tough question to answer, and to be honest, it depends on where you are located.",
                        IsDeleted=false,
                        PostComments=new List<BlogComment>(),
                        PostCategories=new List<BlogCategory>()
                        {
                            new BlogCategory(){
                                Id = new Random().Next(1, 9999999),
                                Title="Technical Skills Growth",
                                IsDeleted=false
                            }
                        }
                    },
                };

                await context.BlogPosts.AddRangeAsync(blogPosts);
            }


            bool hasProjects = await context.Projects.AnyAsync();
            if (!hasProjects) 
            {
                var projects = new List<Project>()
                {
                    new Project
                    {
                        Id=new Random().Next(1,9999999),
                        DatePosted=DateTime.UtcNow.AddDays(3).ToString("MM/dd/yyyy"),
                        DatePostedIso=DateTime.UtcNow,
                        Title="Jax Tides",
                        ProjectImage="/assets/images/Screenshot_2024-10-30_004137.png",
                        AboutProject="This is a neat little application that I made with React.js while using the NOAA tides API for data!",
                        ProjectLink="https://baraiboapex.github.io/jax-tides",
                        IsDeleted=false,
                        ProjectComments=new List<ProjectComment>(),
                        ProjectCategories=new List<ProjectCategory>()
                        {
                            new ProjectCategory(){
                                Id = new Random().Next(1, 9999999),
                                Title="Personal Project",
                                IsDeleted=false
                            }
                        }
                    },
                    new Project
                    {
                        Id=new Random().Next(1,9999999),
                        DatePosted=DateTime.UtcNow.AddDays(2).ToString("MM/dd/yyyy"),
                        DatePostedIso=DateTime.UtcNow,
                        Title="GNBC Japanese Class Tools Suite",
                        ProjectImage=null,
                        AboutProject="For my church, I built a handful of tools for our japanese class using node.js, google apps scripts, and Vue.js!",
                        ProjectLink="https://baraiboapex.github.io/jax-tides",
                        IsDeleted=false,
                        ProjectComments=new List<ProjectComment>(),
                        ProjectCategories=new List<ProjectCategory>()
                        {
                            new ProjectCategory(){
                                Id = new Random().Next(1, 9999999),
                                Title="Not-For-Profit Projects",
                                IsDeleted=false
                            }
                        }
                    },
                    new Project
                    {
                        Id=new Random().Next(1,9999999),
                        DatePosted=DateTime.UtcNow.AddDays(2).ToString("MM/dd/yyyy"),
                        DatePostedIso=DateTime.UtcNow,
                        Title="GNBC Japanese Class Tools Suite",
                        ProjectImage=null,
                        AboutProject="For my church, I built a handful of tools for our japanese class using node.js, google apps scripts, and Vue.js!",
                        ProjectLink="https://baraiboapex.github.io/jax-tides",
                        IsDeleted=false,
                        ProjectComments=new List<ProjectComment>(),
                        ProjectCategories=new List<ProjectCategory>()
                        {
                           new ProjectCategory(){
                                Id = new Random().Next(1, 9999999),
                                Title="Personal Project",
                                IsDeleted=false
                            },
                            new ProjectCategory(){
                                Id = new Random().Next(1, 9999999),
                                Title="Not-For-Profit Projects",
                                IsDeleted=false
                            }
                        }
                    }
                };

                await context.Projects.AddRangeAsync(projects);
            }

            bool hasAboutPosts = await context.Projects.AnyAsync();
            if (!hasAboutPosts)
            {
                var aboutPosts = new List<AboutPost>()
                {
                    new AboutPost
                    {
                        Id = new Random().Next(1, 9999999),
                        DatePostedIso = DateTime.UtcNow,
                        DatePosted = DateTime.UtcNow.ToString("MM/dd/yyyy"),
                        OwnerImage = "/assets/images/owner-image-2.png",
                        Title = "Well, Hi There! How Are You?!",
                        Message = "Hi! My name is Matthew Bailey! I am a professional web designer and developer who specializes in front-end development but also enjoys going full-stack to employ my wizardry on those tech stacks as well be it Node.js or C# .NET! (I'm working on 'getting gud' with python and php as well! ;) ) Besides my absolute banger skillz with tech, I am also a HUGE meat connoisseur and look forward to one day tasting jamon iberico and culatello di zebillio! (Not that you needed to know that) When I am not coding, I am most likely practicing taekwondo, gaming, or just enjoying some time in the great outdoors! Because my pasty white computer nerd skin needs some vitamin D too, right?",
                        IsDeleted = false
                    }

                };
            }

            bool hasSearchableBlogFields = await context.SearchableBlogPostFields.AnyAsync();
            if (!hasSearchableBlogFields)
            {
                var searchableBlogFields = new List<SearchableBlogPostField>()
                {
                    new SearchableBlogPostField
                    {
                        Id = new Random().Next(1, 9999999),
                        BlogPostFieldName="DatePosted"
                    },
                    new SearchableBlogPostField
                    {
                        Id = new Random().Next(1, 9999999),
                        BlogPostFieldName="PostCategories"
                    },
                };

                await context.SearchableBlogPostFields.AddRangeAsync(searchableBlogFields);
            }

            bool hasSearchableProjectFields = await context.SearchableBlogPostFields.AnyAsync();
            if (!hasSearchableProjectFields)
            {
                var searchableProjectFields = new List<SearchableProjectField>()
                {
                    new SearchableProjectField
                    {
                        Id = new Random().Next(1, 9999999),
                        ProjectFieldName="DatePosted"
                    },
                    new SearchableProjectField
                    {
                        Id = new Random().Next(1, 9999999),
                        ProjectFieldName="ProjectCategories"
                    },
                };

                await context.SearchableProjectFields.AddRangeAsync(searchableProjectFields);
            }
            
            await context.SaveChangesAsync();
        }
    }
}
