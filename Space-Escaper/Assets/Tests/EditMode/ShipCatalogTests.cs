using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SpaceEscaper.Tests
{
    public class ShipCatalogTests
    {
        private const string k_CatalogPath = "Assets/Data/Ships/ShipCatalog.asset";

        private ShipCatalog m_catalog;

        [SetUp]
        public void LoadCatalog()
        {
            m_catalog = AssetDatabase.LoadAssetAtPath<ShipCatalog>(k_CatalogPath);
        }

        [Test]
        public void CatalogListsEveryShipOnce()
        {
            string[] shipAssets = AssetDatabase.FindAssets("t:" + nameof(ShipData), new[] { "Assets" });
            HashSet<ShipData> listedShips = new HashSet<ShipData>(m_catalog.Ships);

            Assert.That(m_catalog.Ships, Has.None.Null);
            Assert.That(listedShips.Count, Is.EqualTo(m_catalog.Ships.Count), "A ship is listed twice.");
            Assert.That(listedShips.Count, Is.EqualTo(shipAssets.Length), "A ship asset is missing in the catalog.");
        }

        [Test]
        public void StarterShipComesFirstAndIsFree()
        {
            Assert.That(m_catalog.Ships[0].Price, Is.EqualTo(0));
        }

        [Test]
        public void EveryOtherShipHasPriceAndPriceTag()
        {
            for (int i = 1; i < m_catalog.Ships.Count; i++)
            {
                ShipData ship = m_catalog.Ships[i];
                Assert.That(ship.Price, Is.GreaterThan(0), ship.name);
                Assert.That(ship.PriceTag, Is.Not.Null, ship.name);
            }
        }

        [Test]
        public void EveryShipHasModelAndHangarScale()
        {
            foreach (ShipData ship in m_catalog.Ships)
            {
                Vector3 hangarScale = ship.HangarScale;

                Assert.That(ship.Model, Is.Not.Null, ship.name);
                Assert.That(ship.Model.GetComponentInChildren<Renderer>(), Is.Not.Null, ship.name);
                Assert.That(hangarScale.x * hangarScale.y * hangarScale.z, Is.GreaterThan(0f), ship.name);
            }
        }

        [Test]
        public void EveryShipHasEngineFlame()
        {
            foreach (ShipData ship in m_catalog.Ships)
            {
                Assert.That(ship.EngineFlame, Is.Not.Null, ship.name);
                Assert.That(ship.EngineFlame.GetComponentInChildren<ParticleSystem>(), Is.Not.Null, ship.name);
            }
        }

        [Test]
        public void EveryFamilyHasShips()
        {
            foreach (ShipFamily family in System.Enum.GetValues(typeof(ShipFamily)))
            {
                ShipData firstShip = m_catalog.GetFirstShipOfFamily(family);

                Assert.That(firstShip, Is.Not.Null, family.ToString());
                Assert.That(firstShip.Family, Is.EqualTo(family));
            }
        }
    }
}
