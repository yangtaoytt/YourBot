/*
 Navicat Premium Data Transfer

 Source Server         : mysql
 Source Server Type    : MySQL
 Source Server Version : 80036 (8.0.36)
 Source Host           : localhost:3306
 Source Schema         : yourbot

 Target Server Type    : MySQL
 Target Server Version : 80036 (8.0.36)
 File Encoding         : 65001

 Date: 24/02/2025 21:30:32
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for actor
-- ----------------------------
DROP TABLE IF EXISTS `actor`;
CREATE TABLE `actor`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `creator_uin` int UNSIGNED NOT NULL COMMENT 'actor创建者qq号',
  `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'actior名',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `creator_uin`(`creator_uin` ASC, `name` ASC) USING BTREE COMMENT '同一用户不可有重复的actor'
) ENGINE = InnoDB AUTO_INCREMENT = 12 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for actor_member
-- ----------------------------
DROP TABLE IF EXISTS `actor_member`;
CREATE TABLE `actor_member`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `actor_id` int UNSIGNED NOT NULL COMMENT '对应actor的id',
  `user_uin` int UNSIGNED NOT NULL COMMENT '用户的qq',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `actor_id`(`actor_id` ASC, `user_uin` ASC) USING BTREE COMMENT '一个actor内无重复user',
  CONSTRAINT `actor_member_ibfk_1` FOREIGN KEY (`actor_id`) REFERENCES `actor` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 12 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for actor_member_invitation
-- ----------------------------
DROP TABLE IF EXISTS `actor_member_invitation`;
CREATE TABLE `actor_member_invitation`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `actor_id` int UNSIGNED NOT NULL COMMENT '对应actor的id',
  `user_uin` int UNSIGNED NOT NULL COMMENT '用户的qq',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `actor_id`(`actor_id` ASC, `user_uin` ASC) USING BTREE COMMENT '一个actor内无重复user',
  CONSTRAINT `actor_member_invitation_ibfk_1` FOREIGN KEY (`actor_id`) REFERENCES `actor` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 26 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for face_message
-- ----------------------------
DROP TABLE IF EXISTS `face_message`;
CREATE TABLE `face_message`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `face_id` int NOT NULL COMMENT '表情id',
  `is_large` tinyint NOT NULL COMMENT '是否为大表情',
  `message_chain_id` int UNSIGNED NOT NULL COMMENT '所属msgchain id',
  `sequence` int UNSIGNED NOT NULL COMMENT '在msgchain中序号',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `meme_id`(`message_chain_id` ASC) USING BTREE,
  CONSTRAINT `face_message_ibfk_1` FOREIGN KEY (`message_chain_id`) REFERENCES `group_message_chain` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for group_message_chain
-- ----------------------------
DROP TABLE IF EXISTS `group_message_chain`;
CREATE TABLE `group_message_chain`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `group_uin` int UNSIGNED NOT NULL COMMENT '群组号',
  `sender_uin` int UNSIGNED NULL DEFAULT NULL COMMENT '发送者id',
  `sender_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '发送者名字',
  `sender_avatar` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '发送者头像url',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 191 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for homework
-- ----------------------------
DROP TABLE IF EXISTS `homework`;
CREATE TABLE `homework`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `actor_id` int UNSIGNED NOT NULL COMMENT '所属actor的id',
  `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'homework的名称',
  `deadline` datetime NOT NULL COMMENT '作业截止时间',
  `remind_time` datetime NOT NULL COMMENT '提醒时间等于deadline表示不提醒',
  `is_release` tinyint NOT NULL COMMENT '是否已经进行发布',
  `introduction` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL DEFAULT NULL COMMENT '简单介绍',
  `create_time` datetime NOT NULL COMMENT '作业创建时间',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `actor_id`(`actor_id` ASC, `name` ASC) USING BTREE COMMENT '同一个actir下不可有相同的homework',
  CONSTRAINT `homework_ibfk_1` FOREIGN KEY (`actor_id`) REFERENCES `actor` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 32 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for homework_member
-- ----------------------------
DROP TABLE IF EXISTS `homework_member`;
CREATE TABLE `homework_member`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增的id',
  `homework_id` int UNSIGNED NOT NULL COMMENT '对应homework的id',
  `user_uin` int UNSIGNED NOT NULL COMMENT '已经提交的user的qq号',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `homework_id`(`homework_id` ASC) USING BTREE,
  CONSTRAINT `homework_member_ibfk_1` FOREIGN KEY (`homework_id`) REFERENCES `homework` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for homework_regex
-- ----------------------------
DROP TABLE IF EXISTS `homework_regex`;
CREATE TABLE `homework_regex`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `homework_id` int UNSIGNED NOT NULL COMMENT '对应homework的序号',
  `regex` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '正则表达式',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `homework_id`(`homework_id` ASC) USING BTREE,
  CONSTRAINT `homework_regex_ibfk_1` FOREIGN KEY (`homework_id`) REFERENCES `homework` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 23 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for image_message
-- ----------------------------
DROP TABLE IF EXISTS `image_message`;
CREATE TABLE `image_message`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `path` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '图片路径',
  `message_chain_id` int UNSIGNED NOT NULL COMMENT '所属msgchain id',
  `sequence` int UNSIGNED NOT NULL COMMENT '在msgchain中序号',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `meme_image_message_ibfk_1`(`message_chain_id` ASC) USING BTREE,
  CONSTRAINT `image_message_ibfk_1` FOREIGN KEY (`message_chain_id`) REFERENCES `group_message_chain` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 200 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for meme
-- ----------------------------
DROP TABLE IF EXISTS `meme`;
CREATE TABLE `meme`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `message_chain_id` int NOT NULL COMMENT 'message chain id',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 21 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for multi_message
-- ----------------------------
DROP TABLE IF EXISTS `multi_message`;
CREATE TABLE `multi_message`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `message_chain_id` int UNSIGNED NOT NULL COMMENT '所属msgchain id',
  `sequence` int NOT NULL COMMENT '在msgchain中序号',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `message_group_id`(`message_chain_id` ASC) USING BTREE,
  CONSTRAINT `multi_message_ibfk_1` FOREIGN KEY (`message_chain_id`) REFERENCES `group_message_chain` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 12 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for multi_message_2_message_chain
-- ----------------------------
DROP TABLE IF EXISTS `multi_message_2_message_chain`;
CREATE TABLE `multi_message_2_message_chain`  (
  `multi_message_id` int UNSIGNED NOT NULL COMMENT '在mutimessage中的id',
  `message_chain_id` int UNSIGNED NOT NULL COMMENT '在msgchain中的id',
  `sequence` int NOT NULL COMMENT 'msgchain 在multiple中的序号',
  PRIMARY KEY (`multi_message_id`, `message_chain_id`) USING BTREE,
  INDEX `message_group_id`(`message_chain_id` ASC) USING BTREE,
  CONSTRAINT `multi_message_2_message_chain_ibfk_1` FOREIGN KEY (`multi_message_id`) REFERENCES `multi_message` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `multi_message_2_message_chain_ibfk_2` FOREIGN KEY (`message_chain_id`) REFERENCES `group_message_chain` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for text_message
-- ----------------------------
DROP TABLE IF EXISTS `text_message`;
CREATE TABLE `text_message`  (
  `id` int NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `content` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '文字长度不得超过1024',
  `message_chain_id` int UNSIGNED NOT NULL COMMENT '所属msgchain id',
  `sequence` int UNSIGNED NOT NULL COMMENT '在msgchain中的序号',
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `meme_text_message_ibfk_1`(`message_chain_id` ASC) USING BTREE,
  CONSTRAINT `text_message_ibfk_1` FOREIGN KEY (`message_chain_id`) REFERENCES `group_message_chain` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 7 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for version
-- ----------------------------
DROP TABLE IF EXISTS `version`;
CREATE TABLE `version`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '自增id',
  `version_number` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '版本号',
  `update_time` datetime NOT NULL COMMENT '版本更新时间',
  `version_description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '版本描述',
  PRIMARY KEY (`id` DESC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 4 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

SET FOREIGN_KEY_CHECKS = 1;
