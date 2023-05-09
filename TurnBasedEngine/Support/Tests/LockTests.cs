using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LockTests {
    protected class TestLock : ILock {
        public bool Write { get; set; }
        public byte Dependency { get; set; }
        public bool HasLock;
        public Action<ILock> OnExit;
        public float EstimatedDuration { get { return 0f; } }
        public float StartTime { get; set; }
        public string Name;
        public TestLock() {
            this.Write = false;
            this.Dependency = 0xff;
            this.Name = "";
        }
        public TestLock(bool Write, string name, Dependency Dependency) : this(Write, name, (byte)Dependency) {
        }
        public TestLock(bool Write, string name, byte Dependency) {
            this.Write = Write;
            this.Dependency = Dependency;
            this.Name = name;
        }
        public void Enter(Action<ILock> OnExit) {
            this.OnExit = OnExit;
            HasLock = true;
        }
        public void Complete() {
            Assert.That(OnExit != null, "This Lock hasn't been opened yet!");
            OnExit(this);
        }
        public string DebugString() { return null; }
    }

    public static bool OnlyActiveLocks(params ILock[] expectedLocks) {
        return LockController.Instance.GetOpenLocks().IdenticalContents(expectedLocks);
    }
    
    [Test(TestAbstractionLevel.LowLevel)]
    public static void CreateLock() {
        var testLock = new TestLock();
        LockController.Instance.Request(testLock);
        testLock.Complete();
    }
    
    [Test(TestAbstractionLevel.Foundational)]
    public static void ReadBlocksWrite() {
        var readLock = new TestLock();
        var writeLock = new TestLock() { Write = true };
        LockController.Instance.Request(readLock);
        LockController.Instance.Request(writeLock);
        Assert.That(!writeLock.HasLock, "Write lock shouldn't be able to procede until read lock is done");
        readLock.Complete();
        Assert.That(writeLock.HasLock, "Write lock wasn't able to start after read lock finished");
    }
    
    [Test(TestAbstractionLevel.Foundational)]
    public static void WriteBlocksRead() {
        var readLock = new TestLock();
        var writeLock = new TestLock() { Write = true };
        LockController.Instance.Request(writeLock);
        LockController.Instance.Request(readLock);
        Assert.That(!readLock.HasLock, "Read lock shouldn't be able to procede until write lock is done");
        writeLock.Complete();
        Assert.That(readLock.HasLock, "Read lock wasn't able to start after write lock finished");
    }
    
    [Test(TestAbstractionLevel.Foundational)]
    public static void WriteBlocksWrite() {
        var writeLock = new TestLock() { Write = true };
        var otherWriteLock = new TestLock() { Write = true };
        LockController.Instance.Request(writeLock);
        LockController.Instance.Request(otherWriteLock);
        Assert.That(!otherWriteLock.HasLock, "Write lock shouldn't be able to procede until write lock is done");
        writeLock.Complete();
        Assert.That(otherWriteLock.HasLock, "Write lock wasn't able to start after write lock finished");
    }
    
    [Test(TestAbstractionLevel.Foundational)]
    public static void ReadLocksCoincede() {
        var readLock = new TestLock();
        var otherReadLock = new TestLock();
        LockController.Instance.Request(readLock);
        LockController.Instance.Request(otherReadLock);
        Assert.That(otherReadLock.HasLock, "Read lock erroniously blocked by other read lock");
    }
    
    [Test(TestAbstractionLevel.Foundational)]
    public static void ParentBlocksMutualWrites() {
        //    a
        //   / \
        //  b   c
        var bWrite = new TestLock(true, "b", 0xff);
        var aWrite = new TestLock(true, "a", 0xff);
        var cWrite = new TestLock(true, "c", 0xff);
        LockController.Instance.Request(bWrite);
        LockController.Instance.Request(aWrite);
        LockController.Instance.Request(cWrite);
        Assert.That(OnlyActiveLocks(bWrite), "Both writes allowed to procede despite interceeding global write");
        bWrite.Complete();
        Assert.That(OnlyActiveLocks(aWrite));
        aWrite.Complete();
        Assert.That(OnlyActiveLocks(cWrite));
        cWrite.Complete();
        Assert.That(OnlyActiveLocks());
    }
    
    [Test(TestAbstractionLevel.Foundational)]
    public static void ParentWriteBlocksMutualReads() {
        //    a
        //   / \
        //  b   c
        var bRead = new TestLock(false, "", 0xff);
        var aWrite = new TestLock(true, "", 0xff);
        var cRead = new TestLock(false, "", 0xff);
        LockController.Instance.Request(bRead);
        LockController.Instance.Request(aWrite);
        LockController.Instance.Request(cRead);
        Assert.That(OnlyActiveLocks(bRead), "Both reads allowed to procede despite interceeding global write");
        bRead.Complete();
        Assert.That(OnlyActiveLocks(aWrite));
        aWrite.Complete();
        Assert.That(OnlyActiveLocks(cRead));
        cRead.Complete();
        Assert.That(OnlyActiveLocks());
    }
    
    [Test(TestAbstractionLevel.Foundational)]
    public static void WriteYieldsToReadOnComplete() {
        //    a
        //   / \
        //  b   c
        var bWrite = new TestLock(true, "", 0xff);
        var cRead = new TestLock(false, "", 0xff);
        LockController.Instance.Request(bWrite);
        LockController.Instance.Request(cRead);
        Assert.That(OnlyActiveLocks(bWrite));
        bWrite.Complete();
        Assert.That(OnlyActiveLocks(cRead));
        cRead.Complete();
        Assert.That(OnlyActiveLocks());
    }

    /**
Situation:
Player has a relic that causes a fireball to double cast.  The first fireball hits 3 orcs, one of whom dies.  The second fireball hits the 2 remaining orcs, one of whom dies.  The last orc then casts a spell back at the player.

Imperative pseudocode:
Player.CastingSkill = Fireball
Player.CastingSkill = null
new FireOrb(Player.location)
FireOrb.location = Target
Destroy(FireOrb)
Explosion.Spawn(Target)
Foreach orc in orcs
	orc.TakeDamage()
Relic.Proc()
new FireOrb(Player.location)
FireOrb.location = Target
Destroy(FireOrb)
Explosion.Spawn(Target)
Foreach orc in orcs
	orc.TakeDamage()
SurvivingOrc.CastingSkill = Smite

Model Ids:
World: 1
Player: 2
Fireball: 3
Second orc to die (selected in UI): 4
First Orc to die: 5
Orc that casts Smite: 6
Second Fireball: 7

Lock Sequence:
W 2 (casting animation)
W 1 (spawn fireball)
W 1 (move fireball)
W 1 (destroy fireball)
W 1 (explosion)
W 4 (set health)
R 4 (update HP in UI)
R 4 (hit react)
W 5 (set health)
R 5 (hit react)
R 5 (death animation)
W 6 (set health)
R 6 (hit react)
W 1 (relic animation)
W 1 (spawn fireball)
W 1 (move fireball)
W 1 (destroy fireball)
W 1 (explosion)
W 4 (set health)
R 4 (update HP in UI)
R 4 (hit react)
R 4 (death animation)
R 4 (collapse UI)
W 6 (set health)
R 6 (hit react)
W 1 (turn order)
R 6 (casting animation)
W 2 (set health)
R 2 (UI update for damage from orc)
R 2 (get hit)

Timeline Sequence:
Player casting animation
Fireball moving
Explosion
All 3 orcs hit react, UI updates
1 orc dies
Relic animation
Fireball moving
Explosion
2 orcs hit ract, UI updates
1 orc dies
UI updates, orc casts
    */
    [Test(TestAbstractionLevel.HighLevel)]
    public static void LockStressTest() {
        // Model the imperative code creating and requesting all the locks at once
        var castFireball = new TestLock(true, "castFireball", Dependency.Player); LockController.Instance.Request(castFireball);
        var spawnFireball = new TestLock(true, "spawnFireball", Dependency.All); LockController.Instance.Request(spawnFireball);
        var moveFireball = new TestLock(true, "moveFireball", Dependency.All); LockController.Instance.Request(moveFireball);
        var destroyFireball = new TestLock(true, "destroyFireball", Dependency.All); LockController.Instance.Request(destroyFireball);
        var explosion = new TestLock(true, "explosion", Dependency.All); LockController.Instance.Request(explosion);
        var orc4Damage1 = new TestLock(true, "orc4Damage1", Dependency.Monsters); LockController.Instance.Request(orc4Damage1);
        var orc4UI1 = new TestLock(false, "orc4UI1", Dependency.Monsters); LockController.Instance.Request(orc4UI1);
        var orc4HitReact1 = new TestLock(false, "orc4HitReact1", Dependency.Monsters); LockController.Instance.Request(orc4HitReact1);
        var orc5Damage1 = new TestLock(true, "orc5Damage1", Dependency.Monsters); LockController.Instance.Request(orc5Damage1);
        var orc5HitReact1 = new TestLock(false, "orc5HitReact1", Dependency.Monsters); LockController.Instance.Request(orc5HitReact1);
        var orc6Damage1 = new TestLock(true, "orc6Damage1", Dependency.Monsters); LockController.Instance.Request(orc6Damage1);
        var orc6HitReact1 = new TestLock(false, "orc6HitReact1", Dependency.Monsters); LockController.Instance.Request(orc6HitReact1);
        var orc5Death = new TestLock(true, "orc5Death", Dependency.Monsters); LockController.Instance.Request(orc5Death);
        var orc5DeathAnim = new TestLock(false, "orc5DeathAnim", Dependency.Monsters); LockController.Instance.Request(orc5DeathAnim);
        var relicAnim = new TestLock(true, "relicAnim", Dependency.All); LockController.Instance.Request(relicAnim);
        var spawnFireball2 = new TestLock(true, "spawnFireball2", Dependency.All); LockController.Instance.Request(spawnFireball2);
        var moveFireball2 = new TestLock(true, "moveFireball2", Dependency.All); LockController.Instance.Request(moveFireball2);
        var destroyFireball2 = new TestLock(true, "destroyFireball2", Dependency.All); LockController.Instance.Request(destroyFireball2);
        var explosion2 = new TestLock(true, "explosion2", Dependency.All); LockController.Instance.Request(explosion2);
        var orc4Damage2 = new TestLock(true, "orc4Damage2", Dependency.Monsters); LockController.Instance.Request(orc4Damage2);
        var orc4UI2 = new TestLock(false, "orc4UI2", Dependency.Monsters); LockController.Instance.Request(orc4UI2);
        var orc4HitReact2 = new TestLock(false, "orc4HitReact2", Dependency.Monsters); LockController.Instance.Request(orc4HitReact2);
        var orc6Damage2 = new TestLock(true, "orc6Damage2", Dependency.Monsters); LockController.Instance.Request(orc6Damage2);
        var orc6HitReact2 = new TestLock(false, "orc6HitReact2", Dependency.Monsters); LockController.Instance.Request(orc6HitReact2);
        var orc4Death = new TestLock(true, "orc4Death", Dependency.Monsters); LockController.Instance.Request(orc4Death);
        var orc4DeathAnim = new TestLock(false, "orc4DeathAnim", Dependency.Monsters); LockController.Instance.Request(orc4DeathAnim);
        var turnOrder = new TestLock(true, "turnOrder", Dependency.All); LockController.Instance.Request(turnOrder);
        var orc6CastAnimation = new TestLock(true, "orc6CastAnimation", Dependency.Monsters); LockController.Instance.Request(orc6CastAnimation);
        var playerSetHealth = new TestLock(true, "playerSetHealth", Dependency.Monsters); LockController.Instance.Request(playerSetHealth);
        var playerUIUpdate = new TestLock(false, "playerUIUpdate", Dependency.Monsters); LockController.Instance.Request(playerUIUpdate);
        var playerHitReact = new TestLock(false, "playerHitReact", Dependency.Monsters); LockController.Instance.Request(playerHitReact);

        // Model the timeline playing out
        Assert.That(OnlyActiveLocks(castFireball));
        castFireball.Complete();
        spawnFireball.Complete();
        moveFireball.Complete();
        destroyFireball.Complete();
        explosion.Complete();
        Assert.That(OnlyActiveLocks(orc4Damage1));
        orc4Damage1.Complete();
        orc4UI1.Complete();
        orc4HitReact1.Complete();
        orc5Damage1.Complete();
        orc5HitReact1.Complete();
        orc6Damage1.Complete();
        orc6HitReact1.Complete();
        orc5Death.Complete();
        orc5DeathAnim.Complete();
        Assert.That(OnlyActiveLocks(relicAnim));
        relicAnim.Complete();
        spawnFireball2.Complete();
        moveFireball2.Complete();
        destroyFireball2.Complete();
        explosion2.Complete();
        Assert.That(OnlyActiveLocks(orc4Damage2));
        orc4Damage2.Complete();
        orc4HitReact2.Complete();
        orc4UI2.Complete();
        orc6Damage2.Complete();
        Assert.That(OnlyActiveLocks(orc6HitReact2));
        orc6HitReact2.Complete();
        orc4Death.Complete();
        Assert.That(OnlyActiveLocks(orc4DeathAnim), "Make sure orc 4 has center stage");
        orc4DeathAnim.Complete();
        turnOrder.Complete();
        orc6CastAnimation.Complete();
        playerSetHealth.Complete();
        Assert.That(OnlyActiveLocks(playerUIUpdate, playerHitReact));
        playerUIUpdate.Complete();
        playerHitReact.Complete();
        Assert.That(OnlyActiveLocks(), "Make sure the queue is empty at the end");
    }
}