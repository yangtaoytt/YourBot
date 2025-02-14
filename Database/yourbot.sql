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

 Date: 14/02/2025 15:05:32
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

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
