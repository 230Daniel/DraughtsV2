import React from "react";

import styles from "./Footer.module.css";

export default class Footer extends React.Component {
	render() {
		return (
			<div className={styles.container}>
				<div className={styles.left}>
					<span>© Daniel Baynton 2021</span>
				</div>
				<div className={styles.right}>
					<a href="https://github.com/230Daniel/DraughtsV2" target="_blank" rel="noreferrer">
						<div className={styles.github} />
					</a>
				</div>
			</div>
		);
	}
}
