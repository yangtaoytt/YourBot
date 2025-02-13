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

 Date: 13/02/2025 21:31:28
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for meme
-- ----------------------------
DROP TABLE IF EXISTS `meme`;
CREATE TABLE `meme`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
  `sender_uin` int UNSIGNED NOT NULL COMMENT '发送者',
  `hash_code` int NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `hash_code`(`hash_code` ASC) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 11 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for meme_face_message
-- ----------------------------
DROP TABLE IF EXISTS `meme_face_message`;
CREATE TABLE `meme_face_message`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
  `face_id` int NOT NULL COMMENT '表情id',
  `is_large` tinyint NOT NULL,
  `meme_id` int UNSIGNED NOT NULL,
  `sequence` int UNSIGNED NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `meme_id`(`meme_id` ASC) USING BTREE,
  CONSTRAINT `meme_face_message_ibfk_1` FOREIGN KEY (`meme_id`) REFERENCES `meme` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for meme_image_message
-- ----------------------------
DROP TABLE IF EXISTS `meme_image_message`;
CREATE TABLE `meme_image_message`  (
  `id` int UNSIGNED NOT NULL AUTO_INCREMENT,
  `size` int UNSIGNED NOT NULL,
  `path` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '图片路径',
  `meme_id` int UNSIGNED NOT NULL,
  `sequence` int UNSIGNED NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `meme_image_message_ibfk_1`(`meme_id` ASC) USING BTREE,
  CONSTRAINT `meme_image_message_ibfk_1` FOREIGN KEY (`meme_id`) REFERENCES `meme` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for meme_text_message
-- ----------------------------
DROP TABLE IF EXISTS `meme_text_message`;
CREATE TABLE `meme_text_message`  (
  `id` int NOT NULL AUTO_INCREMENT,
  `content` varchar(800) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '文字长度不得超过800',
  `meme_id` int UNSIGNED NOT NULL,
  `sequence` int UNSIGNED NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `meme_text_message_ibfk_1`(`meme_id` ASC) USING BTREE,
  CONSTRAINT `meme_text_message_ibfk_1` FOREIGN KEY (`meme_id`) REFERENCES `meme` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB AUTO_INCREMENT = 3 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_0900_ai_ci ROW_FORMAT = DYNAMIC;

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
