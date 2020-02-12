using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SocialMediaDB.Models;

namespace learnMVC.Controllers
{
    public class MoviesController : Controller
    {
        private SocialMediaEntities1 db = new SocialMediaEntities1();

        // GET: Movies
        public ActionResult Index(string query)
        {
            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                // attempt to download JSON data as a string
                try
                {
                    json_data = w.DownloadString("http://www.imdb.com/xml/find?json=1&tt=on&q=" + query);
                    dynamic results = JsonConvert.DeserializeObject<dynamic>(json_data);

                    List<NameApprox> names = new List<NameApprox>();

                    var titles = results.title_popular;
                    foreach (var title in titles)
                    {
                        var titlee = title.title;
                        var desc = title.title_description;
                        var id = title.id;

                        NameApprox na = new NameApprox
                        {
                            description = desc,
                            title = titlee,
                            imdbid = id
                        };

                        names.Add(na);

                    }
                    return View(names.ToList());
                }
                catch (Exception)
                {
                    var r = db.MovieDetails.ToList();
                    return View(r);
                }
            }


        }

        //GET: Movies/Details/IMDBID
        public ActionResult MovieDetails(string imdb)
        {
            // This object of MovieDisplay will hold details, cast and reviews
            MovieDisplay movieToDisplay = new MovieDisplay();

            // To be appended for backdrop and poster
            string base_url = "https://image.tmdb.org/t/p/original";
            int movieID = 0;

            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                // Downloading Data of MovieDetails
                try
                {
                    json_data = w.DownloadString("https://api.themoviedb.org/3/movie/" + imdb.ToString() + "?language=en-US&api_key=1359dabdada7feeb1c5d6137ae80011d");
                    dynamic results = JsonConvert.DeserializeObject<dynamic>(json_data);

                    // Method to put all genres into one string
                    string genreInOne = string.Empty;
                    var genres = results.genres;
                    foreach (var gen in genres)
                    {
                        genreInOne += gen.name + ", ";
                    }
                    genreInOne = genreInOne.Remove(genreInOne.Length - 1);
                    genreInOne = genreInOne.Remove(genreInOne.Length - 1);

                    // Putting other variables in their respective places in object of MovieDetails
                    MovieDetail movie = new MovieDetail()
                    {
                        genre = genreInOne,
                        title = results.title,
                        backdrop_path = base_url + results.backdrop_path,
                        budget = results.budget,
                        revenue = results.revenue,
                        overview = results.overview,
                        poster_path = base_url + results.poster_path,
                        release_date = results.release_date,
                        tagline = results.tagline,
                        runtime = results.runtime,
                        vote_average = results.vote_average
                    };

                    // Adding data to MovieDisplay object for final product
                    movieToDisplay.movieInfo = movie;
                    // End addind data to MovieDisplay Object

                    // Adding data of MovieDetails table
                    bool containsItem = db.MovieDetails.Any(item => item.title == movie.title);
                    if (!containsItem)
                    {
                        db.MovieDetails.Add(movie);
                        db.SaveChanges();
                    }

                    movieID = db.MovieDetails.Max(p => p.id); // To store movieID for cast and reviews

                    // now adding cast members
                    // downloading data of cast members

                    // List of cast Members
                    List<Cast> casts = new List<Cast>();
                        try
                        {
                            var json_data1 = w.DownloadString("https://api.themoviedb.org/3/movie/" + imdb.ToString() + "/credits?api_key=1359dabdada7feeb1c5d6137ae80011d");
                            dynamic resultsCast = JsonConvert.DeserializeObject<dynamic>(json_data1);

                            // Method to pull all cast members
                            var castMembers = resultsCast.cast;
                            foreach (var member in castMembers)
                            {
                                Cast newCast = new Cast()
                                {
                                    character = member.character,
                                    name = member.name,
                                    profile_path = base_url + member.profile_path,
                                    MovieID = movieID
                                };

                            if (!containsItem)
                            {
                                db.Casts.Add(newCast);
                            }

                            // Adding data to MovieDisplay Object for final display
                           casts.Add(newCast);
                            // End of adding data to MovieDisplay Object
                        }

                        db.SaveChanges();
                        movieToDisplay.cast = casts;
                        }
                        catch (Exception)
                        {
                            return View();
                        }


                    // End of downloading cast members

                    // Downloading Data of Reviews
                    // List of reviews
                    List<Review> reviewss = new List<Review>();
                    
                        try
                        {
                            var json_data2 = w.DownloadString("https://api.themoviedb.org/3/movie/" + imdb.ToString() + "/reviews?api_key=1359dabdada7feeb1c5d6137ae80011d&language=en-US");
                            dynamic resultsReviews = JsonConvert.DeserializeObject<dynamic>(json_data2);

                            // Method to pull all cast members
                            var reviews = resultsReviews.results;
                            foreach (var review in reviews)
                            {
                                Review newReview = new Review()
                                {
                                    author = review.author,
                                    reviewContent = review.content,
                                    MovieID = movieID
                                };

                            if (!containsItem)
                            {
                                db.Reviews.Add(newReview);
                            }

                            // Adding data to MovieDisplay Object for final display
                            reviewss.Add(newReview);
                            // End of adding data to MovieDisplay Object
                        }

                        db.SaveChanges();
                        movieToDisplay.reviews = reviewss;
                        }
                        catch (Exception)
                        {
                            return View();
                        } 
                    
                    // End of downloading data of Reviews

                    // Showing the movie data
                    return View(movieToDisplay);
                }
                catch (Exception)
                {
                    return View();
                }
            }
        }
        
        // GET: Movies/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,IMDBRating,RTRating,TrailerYTLink,LengthMins,Likes,Recommendations,ReviewsCount,CoverImage,DisplayImage,Director,Category,MovieName")] MovieDetail movie)
        {
            if (ModelState.IsValid)
            {
                db.MovieDetails.Add(movie);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(movie);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
