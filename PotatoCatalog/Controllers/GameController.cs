﻿using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using PotatoCatalog.Models;
using PotatoCatalog.Services;

namespace PotatoCatalog.Controllers
{
    public class GameController : Controller
    {
        private GameServices gameServices;

        public GameController()
        {
            gameServices = new GameServices();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CreateGame()
        {
            GameViewModel model = new GameViewModel {ReleaseDate = DateTime.Now};
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateGame(GameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var imgServices = new ImageServices();
            int gameid = gameServices.CreateGame(model);
            return RedirectToAction("SingleGameView",new {id = gameid});
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult ManageGames()
        {
            var model = gameServices.GetSimpleGamesList();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditGame(int Id)
        {
            var model = gameServices.GetGameViewModelById(Id);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult EditGame(GameViewModel model, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
                var imgServices = new ImageServices();
                if (imgServices.ValidateImage(file))
                {
                    string extension = Path.GetExtension(file.FileName);
                    string relativePath = "~/images/" + model.Id + extension;
                    string physicalPath = Server.MapPath(relativePath);
                    file.SaveAs(physicalPath);
                    model.ImagePath = relativePath;
                }
            gameServices.UpdateGameFromViewModel(model);
            //Get the game from db in case any changes were made
            // trueModel = gameServices.GetGameViewModelById(model.Id);
            return RedirectToAction("SingleGameView", new {id = model.Id});
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult TryDeleteGame(int Id)
        {
            if (gameServices.HasEditions(Id))
            {
                return RedirectToAction("HasEditions");
            }
            else
            {
                var model = gameServices.GetGameViewModelById(Id);
                return PartialView(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult TryDeleteGame(GameViewModel model)
        {
            var isDeleted = gameServices.TryDeleteGame(model);
            if (isDeleted)
            {
                string physicalPath = Server.MapPath(model.ImagePath);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
                return RedirectToAction("ManageGames");
            }
            else
            {
                return RedirectToAction("HasEditions");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult HasEditions()
        {
            return PartialView();
        }

        public ActionResult SingleGameView(int id)
        {
            var model = gameServices.GetCompleteGameViewModelById(id);
            if (model.Id == null || model.Id <1)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
    }
}