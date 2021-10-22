import React from "react";

import styles from "./Piece.Module.css";

export default class Piece extends React.Component {
	render() {
		if (this.props.piece === -1)
			return null;

		return (
			<div className={this.getClassNames()}>
				<div className={styles.inside}>
					{this.props.piece >= 2 &&
						<div className={styles.king} />
					}
				</div>
			</div>
		);
	}

	getClassNames() {
		var classNames = [ styles.piece, this.props.piece % 2 === 0 ? styles.black : styles.white ];
		if (this.props.selected) classNames.push(styles.selected);
		return classNames.join(" ");
	}
}
